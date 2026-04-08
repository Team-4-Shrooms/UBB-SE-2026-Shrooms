using Xunit;

// Disable parallel test execution because DatabaseSingleton shares a single SqlConnection across all tests
[assembly: CollectionBehavior(DisableTestParallelization = true)]
