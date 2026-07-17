import { Pressable, StyleSheet, Text, View } from "react-native";

import { Colors, Radius, Spacing } from "@/constants/prototype-theme";

const ICONS = ["◎", "?", "⚡", "⌕", "✺", "☰"];

export function MenuGrid({ items, onPress }: { items: string[]; onPress: (item: string, index: number) => void }) {
  return (
    <View style={styles.section}>
      <View style={styles.grid}>
        {items.map((item, i) => (
          <Pressable key={item} style={styles.item} onPress={() => onPress(item, i)}>
            <View style={styles.icon}>
              <Text style={styles.iconText}>{ICONS[i] ?? "•"}</Text>
            </View>
            <Text style={styles.label}>{item}</Text>
          </Pressable>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  section: { paddingHorizontal: Spacing.three, paddingVertical: Spacing.two },
  grid: { flexDirection: "row", flexWrap: "wrap", gap: 10 },
  item: {
    width: "47%",
    backgroundColor: Colors.paper,
    borderWidth: 1,
    borderColor: Colors.line,
    borderRadius: Radius.md,
    paddingVertical: 14,
    paddingHorizontal: 10,
    alignItems: "center",
  },
  icon: {
    width: 34,
    height: 34,
    borderRadius: 9,
    backgroundColor: Colors.maroon,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 8,
  },
  iconText: { color: Colors.goldLt, fontSize: 15 },
  label: { fontSize: 11.5, fontWeight: "700", color: Colors.ink, textAlign: "center" },
});
