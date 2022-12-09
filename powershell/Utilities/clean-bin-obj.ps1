Get-ChildItem -Path . -Directory -Filter bin -Recurse | Remove-Item -Force -Recurse -WhatIf
