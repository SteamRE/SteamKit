#!/bin/bash
BUILD_DATE=$(date +"%Y-%m-%d %H:%M:%S %z")
COMMIT_DATE=$(git show -s --format="%ci" HEAD)
COMMIT_SHA=$(git rev-parse --short HEAD)

git diff --quiet && $?
DIRTY="false"
if [ $? ] 
	then 
		DIRTY="true"
fi

VERSION_FILE=${1}/version.cpp

cat > ${VERSION_FILE} <<EOF
#include "version.h"

const char *g_szBuildDate = "${BUILD_DATE}";
const char *g_szBuiltFromCommitSha = "${COMMIT_SHA}";
const char *g_szBuiltFromCommitDate = "${COMMIT_DATE}";
const bool g_bBuiltFromDirty = ${DIRTY};
EOF

