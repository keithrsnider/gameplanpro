#!/bin/bash
# Regenerate the Kiota TypeScript client from the live API OpenAPI spec.
# Usage: ./generate-client.sh
# Requires: API running locally (dotnet run --project Api --launch-profile http)

set -e

SPEC_URL="http://localhost:5115/swagger/v1/swagger.json"
OUTPUT_DIR="ClientApp/src/app/core/api"

echo "Generating Kiota client from $SPEC_URL..."

kiota generate \
  --language typescript \
  --class-name ApiClient \
  --namespace-name GamePlanPro.Api \
  --openapi "$SPEC_URL" \
  --output "$OUTPUT_DIR" \
  --clean-output

echo "Client generated at $OUTPUT_DIR"
