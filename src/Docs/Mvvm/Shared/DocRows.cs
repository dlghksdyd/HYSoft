namespace Docs.Mvvm.Shared
{
    /// <summary>
    /// 옵션 표의 한 행(이름·타입·기본값·설명). 첫 행은 <see cref="IsHeader"/> 로 머리글을 표시한다.
    /// </summary>
    public sealed record OptionRow(string Name, string Type, string Default, string Description, bool IsHeader = false)
    {
        /// <summary>옵션 표의 머리글 행.</summary>
        public static OptionRow Header { get; } = new("Property", "Type", "Default", "Description", true);
    }

    /// <summary>
    /// 멤버 표의 한 행(시그니처와 그 아래 설명). 머리글 행은 없다.
    /// </summary>
    public sealed record MemberRow(string Signature, string Description)
    {
        /// <summary>행 테두리·셀 스타일이 공통으로 보는 값. 멤버 표에는 머리글이 없다.</summary>
        public bool IsHeader => false;
    }
}
