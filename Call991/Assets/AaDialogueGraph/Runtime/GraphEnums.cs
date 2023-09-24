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
    Layer0 = 0,
    Layer1,
    Layer2,
    Layer3,
    Layer4,
}

public enum PhraseEventType
{
    Music,
    RTPC,
    AudioClip,
    VideoClip,
    GameObject,
    Image,
    Projector, // TODO Remove Enum option
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