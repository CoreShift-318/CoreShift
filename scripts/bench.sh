#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
echo "CoreShift benchmark (Release)"
dotnet run -c Release --project src/CoreShift.Console -- --bench
