import { useState } from "react";
import { FlatList, Modal, Pressable, StyleSheet, Text, TextInput, View } from "react-native";

import { Spacing } from "@/constants/theme";
import { useCategories, useDivisions, useSubCategories } from "@/hooks/use-catalog-queries";

interface Filters {
  divisionId: number | null;
  categoryId: number | null;
  subCategoryId: number | null;
}

interface Props extends Filters {
  search: string;
  onSearchChange: (value: string) => void;
  onChange: (filters: Filters) => void;
}

interface Option {
  id: number;
  name: string;
}

function FilterChip({ label, disabled, onPress }: { label: string; disabled?: boolean; onPress: () => void }) {
  return (
    <Pressable style={[styles.chip, disabled && styles.chipDisabled]} disabled={disabled} onPress={onPress}>
      <Text style={[styles.chipText, disabled && styles.chipTextDisabled]} numberOfLines={1}>
        {label}
      </Text>
    </Pressable>
  );
}

function OptionPickerModal({
  visible,
  title,
  options,
  onSelect,
  onClose,
}: {
  visible: boolean;
  title: string;
  options: Option[];
  onSelect: (id: number | null) => void;
  onClose: () => void;
}) {
  return (
    <Modal visible={visible} animationType="slide" transparent onRequestClose={onClose}>
      <Pressable style={styles.modalBackdrop} onPress={onClose}>
        <View style={styles.modalSheet}>
          <Text style={styles.modalTitle}>{title}</Text>
          <FlatList
            data={[{ id: -1, name: `All ${title}` }, ...options]}
            keyExtractor={(item) => String(item.id)}
            renderItem={({ item }) => (
              <Pressable
                style={styles.modalOption}
                onPress={() => {
                  onSelect(item.id === -1 ? null : item.id);
                  onClose();
                }}
              >
                <Text style={styles.modalOptionText}>{item.name}</Text>
              </Pressable>
            )}
          />
        </View>
      </Pressable>
    </Modal>
  );
}

export function ProductFilterBar({
  search,
  onSearchChange,
  divisionId,
  categoryId,
  subCategoryId,
  onChange,
}: Props) {
  const [openPicker, setOpenPicker] = useState<"division" | "category" | "subCategory" | null>(null);

  const divisions = useDivisions();
  const categories = useCategories(divisionId);
  const subCategories = useSubCategories(categoryId);

  const divisionName = divisions.data?.find((d) => d.id === divisionId)?.name ?? "All Divisions";
  const categoryName = categories.data?.find((c) => c.id === categoryId)?.name ?? "All Categories";
  const subCategoryName = subCategories.data?.find((s) => s.id === subCategoryId)?.name ?? "All Sub-Categories";

  return (
    <View>
      <TextInput
        style={styles.search}
        placeholder="Search by product name or SKU..."
        value={search}
        onChangeText={onSearchChange}
      />
      <View style={styles.chipRow}>
        <FilterChip label={divisionId ? divisionName : "All Divisions"} onPress={() => setOpenPicker("division")} />
        <FilterChip
          label={categoryId ? categoryName : "All Categories"}
          disabled={divisionId == null}
          onPress={() => setOpenPicker("category")}
        />
        <FilterChip
          label={subCategoryId ? subCategoryName : "All Sub-Categories"}
          disabled={categoryId == null}
          onPress={() => setOpenPicker("subCategory")}
        />
      </View>

      <OptionPickerModal
        visible={openPicker === "division"}
        title="Division"
        options={divisions.data ?? []}
        onClose={() => setOpenPicker(null)}
        onSelect={(id) => onChange({ divisionId: id, categoryId: null, subCategoryId: null })}
      />
      <OptionPickerModal
        visible={openPicker === "category"}
        title="Category"
        options={categories.data ?? []}
        onClose={() => setOpenPicker(null)}
        onSelect={(id) => onChange({ divisionId, categoryId: id, subCategoryId: null })}
      />
      <OptionPickerModal
        visible={openPicker === "subCategory"}
        title="Sub-Category"
        options={subCategories.data ?? []}
        onClose={() => setOpenPicker(null)}
        onSelect={(id) => onChange({ divisionId, categoryId, subCategoryId: id })}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  search: {
    marginHorizontal: Spacing.three,
    marginTop: Spacing.two,
    borderWidth: 1,
    borderColor: "#E0E1E6",
    borderRadius: 8,
    paddingHorizontal: Spacing.two,
    paddingVertical: 8,
    fontSize: 14,
  },
  chipRow: {
    flexDirection: "row",
    gap: Spacing.one,
    paddingHorizontal: Spacing.three,
    paddingVertical: Spacing.two,
  },
  chip: {
    backgroundColor: "#F0F0F3",
    borderRadius: 999,
    paddingHorizontal: 12,
    paddingVertical: 6,
    flexShrink: 1,
  },
  chipDisabled: { opacity: 0.5 },
  chipText: { fontSize: 13, fontWeight: "600" },
  chipTextDisabled: { color: "#8A8F98" },
  modalBackdrop: { flex: 1, backgroundColor: "rgba(0,0,0,0.4)", justifyContent: "flex-end" },
  modalSheet: {
    backgroundColor: "#fff",
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    maxHeight: "70%",
    padding: Spacing.three,
  },
  modalTitle: { fontSize: 16, fontWeight: "700", marginBottom: Spacing.two },
  modalOption: { paddingVertical: 12, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: "#E0E1E6" },
  modalOptionText: { fontSize: 15 },
});
