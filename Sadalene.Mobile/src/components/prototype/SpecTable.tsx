import { StyleSheet, Text, View } from "react-native";

import { Colors, Spacing } from "@/constants/prototype-theme";

export function SpecTable({ cols, rows }: { cols: string[]; rows: string[][] }) {
  return (
    <View style={styles.section}>
      <View style={styles.table}>
        <View style={styles.headerRow}>
          {cols.map((c) => (
            <Text key={c} style={styles.th}>
              {c}
            </Text>
          ))}
        </View>
        {rows.map((row, i) => (
          <View key={i} style={[styles.row, i % 2 === 1 && styles.rowAlt]}>
            {row.map((cell, j) => (
              <Text key={j} style={styles.td}>
                {cell || "—"}
              </Text>
            ))}
          </View>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  section: { paddingHorizontal: Spacing.three, paddingVertical: Spacing.two },
  table: { borderWidth: 1, borderColor: Colors.line, borderRadius: 8, overflow: "hidden" },
  headerRow: { flexDirection: "row", backgroundColor: Colors.maroon },
  th: {
    flex: 1,
    color: "#d4a520",
    fontSize: 9.5,
    textTransform: "uppercase",
    letterSpacing: 0.5,
    fontWeight: "700",
    padding: 8,
  },
  row: { flexDirection: "row", borderTopWidth: 1, borderTopColor: Colors.line },
  rowAlt: { backgroundColor: "rgba(228,216,190,0.35)" },
  td: { flex: 1, fontSize: 11, color: Colors.ink, padding: 8 },
});
