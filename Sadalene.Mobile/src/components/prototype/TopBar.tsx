import { useRouter } from "expo-router";
import { Pressable, StyleSheet, Text, View } from "react-native";

import { Colors, Fonts, Spacing } from "@/constants/prototype-theme";

interface Props {
  title: string;
  subtitle?: string;
  back?: boolean;
  customerBadge?: string;
}

export function TopBar({ title, subtitle, back, customerBadge }: Props) {
  const router = useRouter();
  return (
    <View style={styles.bar}>
      {back ? (
        <Pressable style={styles.back} onPress={() => router.back()} hitSlop={8}>
          <Text style={styles.backText}>‹</Text>
        </Pressable>
      ) : (
        <View style={styles.backSpacer} />
      )}
      <View style={styles.ttl}>
        <Text style={styles.h}>{title}</Text>
        {subtitle ? <Text style={styles.s}>{subtitle}</Text> : null}
      </View>
      {customerBadge ? (
        <View style={styles.cust}>
          <Text style={styles.custText}>{customerBadge}</Text>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  bar: {
    flexDirection: "row",
    alignItems: "center",
    gap: Spacing.two,
    paddingHorizontal: Spacing.three,
    paddingTop: Spacing.three,
    paddingBottom: Spacing.two,
    backgroundColor: Colors.maroon,
  },
  back: {
    width: 26,
    height: 26,
    borderRadius: 13,
    backgroundColor: "rgba(255,255,255,0.12)",
    alignItems: "center",
    justifyContent: "center",
  },
  backSpacer: { width: 26 },
  backText: { color: Colors.cream, fontSize: 16, lineHeight: 16 },
  ttl: { flex: 1 },
  h: { fontFamily: Fonts.display, fontWeight: "700", fontSize: 18, color: Colors.cream },
  s: { fontSize: 11, color: Colors.cream, opacity: 0.8, marginTop: 1 },
  cust: {
    backgroundColor: Colors.goldLt,
    borderRadius: 20,
    paddingHorizontal: 8,
    paddingVertical: 3,
  },
  custText: { fontSize: 10, color: Colors.maroonDk, fontWeight: "700" },
});
