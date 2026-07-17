import { LinearGradient } from "expo-linear-gradient";
import React from "react";
import { Pressable, StyleSheet, Text, TextInput, View, type StyleProp, type ViewStyle } from "react-native";

import { Colors, Fonts, Radius, Spacing } from "@/constants/prototype-theme";

// ---- Screen scaffolding ----

export function Screen({ children }: { children: React.ReactNode }) {
  return <View style={styles.screen}>{children}</View>;
}

export function Section({ children, style }: { children: React.ReactNode; style?: StyleProp<ViewStyle> }) {
  return <View style={[styles.section, style]}>{children}</View>;
}

// ---- Text / labels ----

export function SectionLabel({ text }: { text: string }) {
  return (
    <Section>
      <Text style={styles.label}>{text}</Text>
    </Section>
  );
}

export function Qtext({ text }: { text: string }) {
  return (
    <Section>
      <Text style={styles.qtext}>{text}</Text>
    </Section>
  );
}

// ---- Banner ----

export function Banner({ text }: { text: string }) {
  return (
    <Section>
      <LinearGradient colors={[Colors.goldLt, Colors.gold]} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={styles.banner}>
        <Text style={styles.bannerText}>{text}</Text>
      </LinearGradient>
    </Section>
  );
}

// ---- Buttons ----

type BtnStyle = "maroon" | "gold" | "ghost";

export function PrimaryButton({
  text,
  onPress,
  variant = "maroon",
  disabled,
}: {
  text: string;
  onPress?: () => void;
  variant?: BtnStyle;
  disabled?: boolean;
}) {
  return (
    <Section>
      <Pressable
        style={[styles.btn, variant === "gold" && styles.btnGold, variant === "ghost" && styles.btnGhost, disabled && styles.btnDisabled]}
        onPress={onPress}
        disabled={disabled}
      >
        <Text style={[styles.btnText, variant === "ghost" && styles.btnGhostText]}>{text}</Text>
      </Pressable>
    </Section>
  );
}

export function LinkRow({ left, right, onLeft, onRight }: { left: string; right: string; onLeft?: () => void; onRight?: () => void }) {
  return (
    <Section style={styles.linkRow}>
      <Pressable onPress={onLeft}>
        <Text style={styles.linkText}>{left}</Text>
      </Pressable>
      <Pressable onPress={onRight}>
        <Text style={styles.linkText}>{right}</Text>
      </Pressable>
    </Section>
  );
}

// ---- Fields ----

export function FieldInput({
  label,
  value,
  onChangeText,
  secure,
  keyboardType,
  placeholder,
}: {
  label: string;
  value: string;
  onChangeText: (v: string) => void;
  secure?: boolean;
  keyboardType?: "default" | "phone-pad" | "number-pad";
  placeholder?: string;
}) {
  return (
    <Section style={{ paddingBottom: 0 }}>
      <Text style={styles.fieldLabel}>{label}</Text>
      <TextInput
        style={styles.fieldBox}
        value={value}
        onChangeText={onChangeText}
        secureTextEntry={secure}
        keyboardType={keyboardType}
        placeholder={placeholder ?? `Enter ${label.toLowerCase()}`}
        placeholderTextColor="#a89b83"
      />
    </Section>
  );
}

// ---- Camera placeholder ----

export function CameraBox({ label, onPress }: { label: string; onPress?: () => void }) {
  return (
    <Section>
      <Pressable style={styles.cam} onPress={onPress}>
        <View style={styles.camFrame} pointerEvents="none" />
        <Text style={styles.camIcon}>▣</Text>
        <Text style={styles.camLabel}>{label}</Text>
      </Pressable>
    </Section>
  );
}

// ---- Note / info boxes ----

export function NoteBox({ text }: { text: string }) {
  return (
    <Section>
      <View style={styles.note}>
        <Text style={styles.noteText}>{text}</Text>
      </View>
    </Section>
  );
}

// ---- List row ----

export function ListRow({ text, onPress }: { text: string; onPress?: () => void }) {
  return (
    <Pressable style={styles.listItem} onPress={onPress}>
      <Text style={styles.listItemText}>{text}</Text>
      <Text style={styles.listItemArrow}>›</Text>
    </Pressable>
  );
}

export function ListSection({ items, onPress }: { items: string[]; onPress?: (item: string, index: number) => void }) {
  return (
    <Section>
      {items.map((item, i) => (
        <ListRow key={item} text={item} onPress={() => onPress?.(item, i)} />
      ))}
    </Section>
  );
}

// ---- Segmented tabs ----

export function SegmentedTabs({ items, active, onChange }: { items: readonly string[]; active: number; onChange: (i: number) => void }) {
  return (
    <Section>
      <View style={styles.tabs}>
        {items.map((t, i) => (
          <Pressable key={t} style={[styles.tab, i === active && styles.tabOn]} onPress={() => onChange(i)}>
            <Text style={[styles.tabText, i === active && styles.tabTextOn]} numberOfLines={1}>
              {t}
            </Text>
          </Pressable>
        ))}
      </View>
    </Section>
  );
}

// ---- Options row (product variant selector) ----

export function OptionRow({ items, active, onChange }: { items: string[]; active: number; onChange: (i: number) => void }) {
  return (
    <Section>
      <View style={styles.optRow}>
        {items.map((t, i) => (
          <Pressable key={t} style={[styles.opt, i === active && styles.optOn]} onPress={() => onChange(i)}>
            <Text style={[styles.optText, i === active && styles.optTextOn]}>{t}</Text>
          </Pressable>
        ))}
      </View>
    </Section>
  );
}

// ---- Total row ----

export function TotalRow({ label, value }: { label: string; value: string }) {
  return (
    <Section>
      <View style={styles.totalRow}>
        <Text style={styles.totalLabel}>{label}</Text>
        <Text style={styles.totalValue}>{value}</Text>
      </View>
    </Section>
  );
}

// ---- Reference number box ----

export function RefNoBox({ label, value }: { label: string; value: string }) {
  return (
    <Section>
      <View style={styles.refno}>
        <Text style={styles.refnoLabel}>{label}</Text>
        <Text style={styles.refnoValue}>{value}</Text>
      </View>
    </Section>
  );
}

// ---- Badge ----

export function Badge({ text }: { text: string }) {
  return (
    <Section>
      <View style={styles.badge}>
        <Text style={styles.badgeText}>{text}</Text>
      </View>
    </Section>
  );
}

// ---- Search bar ----

export function SearchBar({ value, onChangeText, placeholder }: { value: string; onChangeText: (v: string) => void; placeholder: string }) {
  return (
    <Section>
      <View style={styles.searchbar}>
        <Text style={styles.searchIcon}>⌕</Text>
        <TextInput
          style={styles.searchInput}
          value={value}
          onChangeText={onChangeText}
          placeholder={placeholder}
          placeholderTextColor="#a89b83"
        />
      </View>
    </Section>
  );
}

// ---- Dropdown (static, tap opens a picker elsewhere) ----

export function DropdownBox({ text, onPress }: { text: string; onPress?: () => void }) {
  return (
    <Section>
      <Pressable style={styles.dropdown} onPress={onPress}>
        <Text style={styles.dropdownText}>{text}</Text>
        <Text style={styles.dropdownChevron}>▾</Text>
      </Pressable>
    </Section>
  );
}

// ---- Q&A card ----

export function QAItem({ question, answer }: { question: string; answer: string }) {
  return (
    <Section>
      <View style={styles.qaCard}>
        <Text style={styles.qaQuestion}>{question}</Text>
        <Text style={styles.qaAnswer}>{answer}</Text>
      </View>
    </Section>
  );
}

// ---- Row of two ghost buttons ----

export function TwoButtonRow({ a, b, onA, onB }: { a: string; b: string; onA?: () => void; onB?: () => void }) {
  return (
    <View style={[styles.section, styles.twoBtnRow]}>
      <Pressable style={[styles.btn, styles.btnGhost, { flex: 1, marginTop: 0 }]} onPress={onA}>
        <Text style={styles.btnGhostText}>{a}</Text>
      </Pressable>
      <Pressable style={[styles.btn, styles.btnGhost, { flex: 1, marginTop: 0 }]} onPress={onB}>
        <Text style={styles.btnGhostText}>{b}</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: Colors.cream },
  section: { paddingHorizontal: Spacing.three, paddingVertical: Spacing.two },
  label: { fontSize: 10, textTransform: "uppercase", letterSpacing: 1, color: Colors.grey, fontWeight: "700" },
  qtext: { fontSize: 12, color: Colors.ink, lineHeight: 18 },

  banner: {
    borderRadius: Radius.md,
    padding: Spacing.three,
    alignItems: "center",
  },
  bannerText: { fontFamily: Fonts.display, fontWeight: "700", fontSize: 16, color: Colors.maroonDk, textAlign: "center" },

  btn: {
    backgroundColor: Colors.maroon,
    borderRadius: Radius.md,
    paddingVertical: 13,
    alignItems: "center",
    marginTop: Spacing.one,
  },
  btnGold: { backgroundColor: Colors.gold },
  btnGhost: { backgroundColor: "transparent", borderWidth: 1.5, borderColor: Colors.maroon },
  btnDisabled: { opacity: 0.5 },
  btnText: { color: Colors.goldLt, fontWeight: "700", fontSize: 13, letterSpacing: 0.3 },
  btnGhostText: { color: Colors.maroon, fontWeight: "700", fontSize: 13 },

  linkRow: { flexDirection: "row", justifyContent: "space-between" },
  linkText: { fontSize: 11, color: Colors.maroon, fontWeight: "600" },

  fieldLabel: { fontSize: 11, color: Colors.grey, fontWeight: "600", marginBottom: 4 },
  fieldBox: {
    borderWidth: 1.5,
    borderColor: Colors.line,
    backgroundColor: Colors.paper,
    borderRadius: 9,
    paddingHorizontal: 12,
    paddingVertical: 11,
    fontSize: 13,
    color: Colors.ink,
  },

  cam: {
    backgroundColor: "#111",
    borderRadius: Radius.lg,
    height: 210,
    alignItems: "center",
    justifyContent: "center",
  },
  camFrame: {
    position: "absolute",
    top: 14,
    left: 14,
    right: 14,
    bottom: 14,
    borderWidth: 2,
    borderColor: "rgba(212,165,32,0.6)",
    borderStyle: "dashed",
    borderRadius: 10,
  },
  camIcon: { fontSize: 30, color: "#d8c7a3", marginBottom: 6 },
  camLabel: { fontSize: 11, letterSpacing: 1.2, textTransform: "uppercase", color: "#d8c7a3" },

  note: {
    backgroundColor: "rgba(184,134,11,0.08)",
    borderWidth: 1,
    borderColor: "rgba(184,134,11,0.35)",
    borderRadius: 9,
    padding: 11,
  },
  noteText: { fontSize: 11, color: "#6b4c14", fontWeight: "600" },

  listItem: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    backgroundColor: Colors.paper,
    borderWidth: 1,
    borderColor: Colors.line,
    borderRadius: Radius.md,
    paddingHorizontal: 14,
    paddingVertical: 13,
    marginBottom: 9,
  },
  listItemText: { fontWeight: "700", fontSize: 13, color: Colors.ink },
  listItemArrow: { color: Colors.gold, fontSize: 17 },

  tabs: { flexDirection: "row", gap: 6 },
  tab: {
    flex: 1,
    alignItems: "center",
    paddingVertical: 8,
    paddingHorizontal: 4,
    borderRadius: 20,
    borderWidth: 1.5,
    borderColor: Colors.maroon,
  },
  tabOn: { backgroundColor: Colors.maroon },
  tabText: { fontSize: 10.5, fontWeight: "700", color: Colors.maroon },
  tabTextOn: { color: Colors.goldLt },

  optRow: { flexDirection: "row", gap: 8 },
  opt: {
    flex: 1,
    alignItems: "center",
    borderWidth: 1.5,
    borderColor: Colors.line,
    borderRadius: 8,
    paddingVertical: 9,
  },
  optOn: { borderColor: Colors.gold, backgroundColor: "rgba(184,134,11,0.1)" },
  optText: { fontSize: 10.5, fontWeight: "700", color: Colors.ink },
  optTextOn: { color: Colors.maroon },

  totalRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    backgroundColor: Colors.paper,
    borderWidth: 1.5,
    borderColor: Colors.gold,
    borderStyle: "dashed",
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 12,
  },
  totalLabel: { fontWeight: "700", color: Colors.ink },
  totalValue: { fontFamily: Fonts.display, fontSize: 20, fontWeight: "700", color: Colors.maroon },

  refno: { backgroundColor: Colors.maroon, borderRadius: Radius.md, padding: Spacing.three, alignItems: "center" },
  refnoLabel: { fontSize: 9.5, textTransform: "uppercase", letterSpacing: 1.2, color: Colors.goldLt, opacity: 0.8 },
  refnoValue: { fontFamily: Fonts.display, fontSize: 19, fontWeight: "700", color: Colors.goldLt, marginTop: 3 },

  badge: {
    alignSelf: "flex-start",
    backgroundColor: Colors.maroon,
    borderRadius: 20,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  badgeText: { fontSize: 9.5, fontWeight: "700", color: Colors.goldLt, letterSpacing: 0.3 },

  searchbar: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    backgroundColor: Colors.paper,
    borderWidth: 1.5,
    borderColor: Colors.line,
    borderRadius: 22,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  searchIcon: { color: "#a89b83" },
  searchInput: { flex: 1, fontSize: 13, color: Colors.ink },

  dropdown: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    backgroundColor: Colors.paper,
    borderWidth: 1.5,
    borderColor: Colors.line,
    borderRadius: 9,
    paddingHorizontal: 12,
    paddingVertical: 12,
  },
  dropdownText: { fontSize: 12.5, color: "#a89b83" },
  dropdownChevron: { color: "#a89b83" },

  qaCard: {
    backgroundColor: Colors.paper,
    borderWidth: 1,
    borderColor: Colors.line,
    borderRadius: Radius.md,
    padding: 14,
    gap: 6,
  },
  qaQuestion: { fontWeight: "700", fontSize: 13, color: Colors.ink },
  qaAnswer: { fontSize: 12, color: Colors.grey, lineHeight: 17 },

  twoBtnRow: { flexDirection: "row", gap: 8 },
});
