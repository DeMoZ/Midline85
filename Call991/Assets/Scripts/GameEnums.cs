public enum ScreenPlace
{
    None,
    TopLeft,
    MiddleLeft,
    BottomLeft,
    TopRight,
    MiddleRight,
    BottomRight
}

public enum Person
{
    UNKNOWN,
    ELENA,
    LARA,
    JACK,
}

public enum OnPhraseEnd
{
    Nothing = 0,
    HideOnEnd = 1,
}

public enum TextAppear
{
    Word,
    Letters,
    Pop,
    Fade,
}

public enum NextIs
{
    Phrase,
    Choices,
    LevelEnd,
}

public enum SoundUiTypes
{
    ChoiceButton,
    MenuButton,
    Timer
}

public enum PhraseEventTypes
{
    Music,
    Video,
    Sfx,
    Vfx,
    LoopSfx,
    LoopVfx,
    LevelEnd,
}