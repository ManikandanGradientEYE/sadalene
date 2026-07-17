import { ScrollView, StyleSheet, Text, View } from "react-native";

import { Colors, Spacing } from "@/constants/prototype-theme";

const STORY_LABELS = ["New", "Sarees", "Jacquard", "Prints", "Digital", "Sets"];

export function Stories() {
  return (
    <View style={styles.section}>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.row}>
        {STORY_LABELS.map((label) => (
          <View key={label} style={styles.story}>
            <View style={styles.ring}>
              <View style={styles.inner} />
            </View>
            <Text style={styles.label}>{label}</Text>
          </View>
        ))}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  section: { paddingVertical: Spacing.two },
  row: { flexDirection: "row", gap: 10, paddingHorizontal: Spacing.three },
  story: { width: 58, alignItems: "center" },
  ring: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: Colors.gold,
    padding: 2.5,
    alignItems: "center",
    justifyContent: "center",
  },
  inner: { width: "100%", height: "100%", borderRadius: 24, backgroundColor: Colors.line },
  label: { fontSize: 8.5, color: Colors.grey, marginTop: 3 },
});
