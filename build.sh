#!/usr/bin/env bash
# Builds the showcase for Cloudflare Pages, which runs this on every push to the `docs` branch
# (Build command: bash build.sh, Build output directory: publish/wwwroot). See RELEASE.md.
#
# The wasm-tools workload is deliberately not installed: relinking dotnet.native.wasm saves about
# 30 KB of a 4.5 MB (Brotli) download and would add its install time to every build.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

DOTNET_INSTALL_DIR="${HOME}/.dotnet"

echo "==> Installing the .NET SDK from global.json"
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --jsonfile global.json --install-dir "${DOTNET_INSTALL_DIR}"
export DOTNET_ROOT="${DOTNET_INSTALL_DIR}"
export PATH="${DOTNET_INSTALL_DIR}:${PATH}"
dotnet --version

echo "==> Publishing the showcase (Release)"
dotnet publish samples/MudShadcn.Showcase -c Release -o publish

echo "==> Done. The static site is in ./publish/wwwroot"
