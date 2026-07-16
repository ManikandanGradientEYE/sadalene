import { StyleSheet, Text, View } from "react-native";

import { Spacing } from "@/constants/theme";

export function ErrorBanner({ message }: { message: string }) {
  return (
    <View style={styles.container}>
      <Text style={styles.text}>{message}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#FDECEA",
    borderRadius: 8,
    padding: Spacing.two,
    marginHorizontal: Spacing.three,
    marginVertical: Spacing.two,
  },
  text: {
    color: "#B3261E",
    fontSize: 14,
  },
});
