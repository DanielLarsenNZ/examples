# Docker cheatsheet

[How to install Docker on Ubuntu, and getting started][install-docker]

## Docker in WSL2

If you don't want to/can't install Docker Desktop (on Windows) you can still do most Docker things in WSL2. Follow the instructions to [install Docker on Ubuntu][install-docker] in WSL2 and then run the daemon manually:

```bash
sudo dockerd
```

## Check a Docker container image for updates:

```bash
docker pull daniellarsennz/helloaspdotnetcore:latest
docker run -it --entrypoint /bin/bash daniellarsennz/helloaspdotnetcore
# (logs in to container interactively as root). Now you can run bash commands
apt list --upgradeable
```

<!-- Links -->
[install-docker]:https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-20-04
