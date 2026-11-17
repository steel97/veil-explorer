#!/bin/sh
set -e

export NODE_ENV=production
export NITRO_PRESET=node_server
export NUXT_PUBLIC_SITE_URL=https://explorer.veil-project.com

pnpm run build
