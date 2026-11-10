using Xunit;

// 실제 소켓·포트·수십 MB 파일을 쓰는 시험이라 순서대로 돌린다 — 병렬이면 포트·디스크가 겹치고
// [Fact(Timeout)] 상한이 다른 시험의 부하에 흔들린다.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
