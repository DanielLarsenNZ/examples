#!/bin/bash

# Exit on error
set -e

for group in $( az group list --query "[?starts_with(name, 'msdocs' )].name" -o tsv )
do
    echo "Deleting $group ..."
    az group delete -n $group -y --no-wait
done
