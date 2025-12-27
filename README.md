# Federer

An HTTP file server meant primarely for streaming media to devices on the local network.

Like the tennis player, it's a server that's efficient for its size (zero dependencies). Elegant (as long as you don't read the code). And it will probably choke from time to time (2019).

Inspired while taking ThePrimeagen's course [Learn the HTTP Protocol in Go](https://www.boot.dev/courses/learn-http-protocol-golang) with C#. I basically got sidetracked, as I was always curious how streaming media via HTTP works, and why seeking videos in particular was not possible in some servers. 

## Installation

### Via .NET Tool (Recommended)

```bash
dotnet tool install --global federer
```

### Via Homebrew (macOS/Linux)

```bash
brew tap ptrglbvc/tap
brew install federer
```

### Manual Download

Download the latest release from [GitHub Releases](https://github.com/ptrglbvc/federer/releases).

### Installation Script

```bash
curl -fsSL https://raw.githubusercontent.com/ptrglbvc/federer/main/install.sh | bash
```

## Usage

```bash
# Basic usage
federer /video=/path/to/video.mp4

# Multiple routes
federer /=/path/to/video.mp4 /music=/path/to/song.mp3

# Custom port
federer /doc=/path/to/manual.pdf /vid=/path/to/tutorial.mp4 /goat=path/to/djokovic-best-points.mp4

# Show help
federer --help
```

## Features

- Lightweight and fast
- Stream large files without loading into memory
- Range request support (video seeking, resume downloads)
- Simple route-based configuration
- Zero dependencies outside the standard library for the tcp listener.

## License

MIT
