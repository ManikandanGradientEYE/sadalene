import { StyleSheet, Text, View } from "react-native";

import { Colors, Fonts, Radius, Spacing } from "@/constants/prototype-theme";

export function RewardHero({ points }: { points: string }) {
  return (
    <View style={styles.section}>
      <View style={styles.hero}>
        <Text style={styles.points}>{points}</Text>
        <Text style={styles.label}>Reward Points</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  section: { paddingHorizontal: Spacing.three, paddingVertical: Spacing.two },
  hero: { backgroundColor: Colors.maroonDk, borderRadius: Radius.lg, padding: 20, alignItems: "center" },
  points: { fontFamily: Fonts.display, fontSize: 30, fontWeight: "700", color: Colors.goldLt },
  label: { fontSize: 10, opacity: 0.75, letterSpacing: 1.5, textTransform: "uppercase", color: Colors.goldLt },
});
