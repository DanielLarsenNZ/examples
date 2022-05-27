# Bash cheatsheet

Behold! The mystical world of Bash. Yet another Bash cheatsheet for PowerShell devs by @DanielLarsenNZ.

## Bash scripts

```bash
# Grant execute permission before you run
chmod +x ./myscript.sh
```

### Set Error Preference

But the hash bang on very first line to declare this file as a bash shell script.

```bash 
#!/bin/bash

# Exit on error
set -e
```

## Strings

```bash
# Remove 'https://'
spaUrl=${spaFQUrl/https:\/\//}

# Remove forward slash
spaUrl=${spaUrl/\//}
```

## if then else

```bash
# Test if a (string) variable is empty and echo

#   Interpolating the variable in a string forces a unary comparison

if [ "$CUSTOM_DOMAIN_NAME" == '' ]; 
    then echo "CUSTOM_DOMAIN_NAME is not set."
    else echo "CUSTOM_DOMAIN_NAME = $CUSTOM_DOMAIN_NAME"
fi


# Test if a (string) variable is empty and then do several things

if [ "$CUSTOM_DOMAIN_NAME" == '' ]; 
    then 
        echo "Doing some things."

        # Do the things
        #...
    else
        echo "Not doing the things"
fi
```

## Split path & file

`dirname` and `basename` are handy built-in functions for extracting path and file name from a file-path, e.g.

```bash
file=scripts/test.sh
cd $(dirname "${file}")
chmod +X $(basename "${file}")
/bin/bash $(basename "${file}")
```

## For each file

```bash
for file in $files
do
    echo "Running $file ..."
    # ...
done
```

## Test if filename ends in ...

```bash
if [[ "$file" == *.sh ]]
then
    echo "Running $file ..."
    # ...
fi
```

## Echo a warning

This does not write to stderr. There is no Warning out. It just prints the text in Yellow.

```bash
# The text will be yellow
echo -e "\033[33mCUSTOM_DOMAIN_NAME environment variable is not set\033[0m"
```

## az CLI

```bash
# Get the primaryEndpoints.web property from the response and set it to a variable

spaUrl=$( az storage account show -n $storage --query 'primaryEndpoints.web' -o tsv )
```
### Delete all Resource Groups that start with msdocs

```bash
for group in $( az group list --query "[?starts_with(name, 'msdocs' )].name" -o tsv )
do
    echo "Deleting $group ..."
    az group delete -n $group -y --no-wait
done
```
