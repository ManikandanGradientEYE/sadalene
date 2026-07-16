import { useRouter } from "expo-router";
import { useDeferredValue, useMemo, useState } from "react";
import { ActivityIndicator, FlatList, Pressable, StyleSheet, Text, TextInput, View } from "react-native";

import { useActingCustomer } from "@/actingCustomer/useActingCustomer";
import { useAuth } from "@/auth/useAuth";
import { EmptyState } from "@/components/empty-state";
import { ErrorBanner } from "@/components/error-banner";
import { Spacing } from "@/constants/theme";
import { useCustomerSearch, useMyCustomers } from "@/hooks/use-customer-queries";
import { errorMessage } from "@/lib/queryClient";
import type { CustomerListItemDto } from "@/types/dto";

export default function CustomerPickerScreen() {
  const { auth, signOut } = useAuth();
  const { setActingCustomer } = useActingCustomer();
  const router = useRouter();
  const [query, setQuery] = useState("");
  const deferredQuery = useDeferredValue(query);

  const isAgent = auth?.identityType === "Agent";

  const myCustomers = useMyCustomers(isAgent);
  const searchResults = useCustomerSearch(deferredQuery, !isAgent);

  const filteredAgentCustomers = useMemo(() => {
    if (!isAgent) return [];
    const term = deferredQuery.trim().toLowerCase();
    const all = myCustomers.data ?? [];
    if (term.length === 0) return all;
    return all.filter((c) => c.fullName.toLowerCase().includes(term) || c.phone.includes(term));
  }, [isAgent, myCustomers.data, deferredQuery]);

  const list = isAgent ? filteredAgentCustomers : searchResults.data ?? [];
  const isLoading = isAgent ? myCustomers.isLoading : searchResults.isFetching;
  const error = isAgent ? myCustomers.error : searchResults.error;

  function selectCustomer(customer: CustomerListItemDto) {
    setActingCustomer({ id: customer.id, name: customer.fullName });
    router.replace("/catalog");
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>{isAgent ? "Select Customer" : "Search Customer"}</Text>
        <Pressable onPress={signOut}>
          <Text style={styles.logout}>Log out</Text>
        </Pressable>
      </View>

      <TextInput
        style={styles.search}
        placeholder={isAgent ? "Filter your customers..." : "Search by name or phone..."}
        value={query}
        onChangeText={setQuery}
      />

      {error ? <ErrorBanner message={errorMessage(error)} /> : null}
      {isLoading ? <ActivityIndicator style={styles.loading} /> : null}

      <FlatList
        data={list}
        keyExtractor={(item) => String(item.id)}
        renderItem={({ item }) => (
          <Pressable style={styles.row} onPress={() => selectCustomer(item)}>
            <Text style={styles.name}>{item.fullName}</Text>
            <Text style={styles.meta}>
              {item.phone}
              {item.city ? ` · ${item.city}` : ""}
            </Text>
          </Pressable>
        )}
        ListEmptyComponent={
          !isLoading ? (
            <EmptyState
              message={
                isAgent
                  ? "You have no customers yet."
                  : query.trim().length === 0
                    ? "Search for a customer to continue."
                    : "No customers found."
              }
            />
          ) : null
        }
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: Spacing.three,
    paddingTop: Spacing.five,
    paddingBottom: Spacing.two,
  },
  title: { fontSize: 22, fontWeight: "700" },
  logout: { fontSize: 14, color: "#B3261E", fontWeight: "600" },
  search: {
    marginHorizontal: Spacing.three,
    borderWidth: 1,
    borderColor: "#E0E1E6",
    borderRadius: 10,
    paddingHorizontal: Spacing.three,
    paddingVertical: 12,
    fontSize: 15,
    marginBottom: Spacing.two,
  },
  loading: { marginTop: Spacing.three },
  row: {
    paddingHorizontal: Spacing.three,
    paddingVertical: Spacing.two,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: "#E0E1E6",
  },
  name: { fontSize: 16, fontWeight: "600" },
  meta: { fontSize: 13, color: "#8A8F98", marginTop: 2 },
});
