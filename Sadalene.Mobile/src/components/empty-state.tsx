import { StyleSheet, Text, View } from "react-native";

import { Spacing } from "@/constants/theme";

export function EmptyState({ message }: { message: string }) {
  return (
    <View style={styles.container}>
      <Text style={styles.text}>{message}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    padding: Spacing.four,
  },
  text: {
    color: "#8A8F98",
    fontSize: 15,
    textAlign: "center",
  },
});
