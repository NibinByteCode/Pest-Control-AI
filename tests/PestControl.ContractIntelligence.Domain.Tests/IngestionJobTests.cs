using PestControl.ContractIntelligence.Domain.Documents;

namespace PestControl.ContractIntelligence.Domain.Tests;

public sealed class IngestionJobTests
{
    [Fact]
    public void FailedJobCanBeRetriedAndAttemptCountIncreases()
    {
        var job = IngestionJob.Queue(Guid.NewGuid());

        job.Start();
        job.Fail("provider-timeout");
        job.Start();

        Assert.Equal(IngestionJobStatus.Running, job.Status);
        Assert.Equal(2, job.AttemptCount);
        Assert.Null(job.FailureCode);
    }

    [Fact]
    public void SucceededJobCannotBeStartedAgain()
    {
        var job = IngestionJob.Queue(Guid.NewGuid());
        job.Start();
        job.Succeed();

        Assert.Throws<InvalidOperationException>(job.Start);
    }
}
