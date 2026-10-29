#!/bin/sh
set -e

export NODE_ENV=production
export NITRO_PRESET=node_server

pnpm run build
