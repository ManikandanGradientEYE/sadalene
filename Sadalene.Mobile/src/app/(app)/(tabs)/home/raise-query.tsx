import { useState } from "react";
import { ScrollView } from "react-native";

import { FieldInput, PrimaryButton, QAItem, Screen, SectionLabel } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_QUERIES } from "@/mock/reports";

export default function RaiseQueryScreen() {
  const [question, setQuestion] = useState("");
  const [queries, setQueries] = useState(MOCK_QUERIES);

  function handleSubmit() {
    if (question.trim().length === 0) return;
    setQueries([{ question: question.trim(), answer: "Awaiting a reply from our staff…" }, ...queries]);
    setQuestion("");
  }

  return (
    <Screen>
      <TopBar title="Raise Query" subtitle="Support / Q&A" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <FieldInput label="Your Question" value={question} onChangeText={setQuestion} placeholder="Type your question…" />
        <PrimaryButton text="Submit Query" variant="gold" onPress={handleSubmit} />
        <SectionLabel text="Previous Queries" />
        {queries.map((qa, i) => (
          <QAItem key={i} question={qa.question} answer={qa.answer} />
        ))}
      </ScrollView>
    </Screen>
  );
}
