public enum AaNodeType
{
    PhraseNode,
    EntryNode,
    ChoiceNode,
    ForkNode,
    CountNode,
    EndNode,
}

public enum LanguageOperationType
{
    Add,
    Change,
    Remove,
}

public enum PhraseEventLayer
{
    Effects,
    Single1,
    Single2,
    Multiple,
}

public enum PhraseEventType
{
    Music,
    AudioClip,
    VideoClip,
    GameObject,
}

public enum CaseType
{
    AndWord,
    NoWord,
}

public enum EndType
{
    AndEnd,
    NoEnd,
}

public enum CountType
{
    Sum,
}