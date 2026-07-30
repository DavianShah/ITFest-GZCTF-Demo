namespace GZCTF.Models.Request.Edit;

public class AiUsageDisclosureModel
{
    public int SubmissionId { get; set; }

    public DateTimeOffset SubmitTimeUtc { get; set; }

    public string Team { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Challenge { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public AnswerResult Status { get; set; }

    public string AiUsageDisclosure { get; set; } = string.Empty;

    public string? SolverFileName { get; set; }

    public long? SolverFileSize { get; set; }

    public bool HasSolverFile { get; set; }
}
