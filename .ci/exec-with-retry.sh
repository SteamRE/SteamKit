#!/bin/bash -x

# Try 3 times
for iter in {1..3}
do
	("$@")
	if [[ $? -eq 0 ]]
	then
		exit 0
	fi
done