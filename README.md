# OpsDesk

> An internal IT support assistant in .NET 10: Blazor chat, answers from documentation with citations (RAG over pgvector), tools served over MCP, and ticket creation that only happens after human approval. Built around a leakage-controlled evaluation suite. **Work in progress.**

OpsDesk is an internal IT support assistant built with .NET. The project is designed to make its answers and actions inspectable: documentation citations show the source of an answer, a tool trace shows what the assistant called, and ticket creation requires a human approval step.

## What will make this project distinctive

- **Paraphrase robustness:** the evaluation set will ask each documentation question in source-like language and in natural employee language. Retrieval results will be reported separately, including the gap between them.
- **Controlled actions:** the MCP `create_ticket` tool will only propose a ticket. A server-side policy and a separate human approval endpoint will control the actual write.
- **Evidence a reviewer can inspect:** tool calls, citation links, injection cases, and measured evaluation outputs will be available in the UI or under `results/`.

These are project goals. The chat, MCP tools, approval flow, and evaluation suite are not implemented yet.

## Working now

- A .NET solution with a core library and xUnit tests.
- A local PostgreSQL container with pgvector enabled.
- A Markdown chunker that splits at headings, overlaps long sections, keeps citation metadata, and produces stable content keys.

## Run the current slice

From the repository root:

```sh
python3 -c 'import secrets; from pathlib import Path; p = Path(".env"); p.exists() or p.write_text("OPSDESK_DB_PASSWORD=" + secrets.token_urlsafe(32) + "\n"); p.chmod(0o600)'
docker compose up -d --wait postgres
dotnet test OpsDesk.slnx
```

The first command creates a local password in `.env` if needed; Git ignores that file. PostgreSQL listens on `127.0.0.1:5433` for local development.

## Verified results and versions

The current test run is saved in [`results/core-tests-step3.txt`](results/core-tests-step3.txt). Tool versions are recorded in [`results/setup.txt`](results/setup.txt), the installed pgvector version and query check in [`results/postgres-step2.txt`](results/postgres-step2.txt), the local port in [`results/postgres-port-step3.txt`](results/postgres-port-step3.txt), and test package versions in [`results/packages-step3.txt`](results/packages-step3.txt) with their NuGet checks in [`results/nuget-search-step3.txt`](results/nuget-search-step3.txt). No retrieval, answer-quality, security, or latency result has been measured yet.

Versions used so far: .NET SDK 10.0.401, Docker 28.0.1, Ollama 0.20.7, pgvector 0.8.7, xUnit 2.9.3, xUnit Visual Studio runner 3.1.4, Microsoft.NET.Test.Sdk 17.14.1, and coverlet.collector 6.0.4. The test-package versions came from the installed xUnit template and were checked against NuGet before use.
