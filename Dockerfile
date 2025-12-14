# Use .NET SDK for building Windows applications
FROM mcr.microsoft.com/dotnet/sdk:10.0

# Install required tools
RUN apt-get update && apt-get install -y \
    zip \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Entry point will be specified when running the container
CMD ["/bin/bash"]
