
# ZipServe

ZipServe is a .NET-based file server application designed to automatically zip selected files and facilitate their download. This solution simplifies the process of transferring multiple files by compressing them into a single archive, reducing both the size and number of downloads.

## System Requirements

- .NET 8.0 or higher
- Windows, Linux, or macOS

## Installation

Follow these steps to get ZipServe up and running on your system:

1. **Clone the repository:**
```
git clone https://github.com/jovanovicdima/zipserve.git
cd zipserve
```

2. **Install dependencies:**
```
dotnet restore
```

3. **Create a directory for files:**
   Ensure a directory named `Files` exists within the root directory of the project to hold the files to be zipped and downloaded.
```
mkdir Files
```

4. **Place your files:**
   Copy any files you wish to be downloadable into the `./Files/` directory.

## Usage

### Starting the Server

To run ZipServe, execute the following command in the terminal:
```
dotnet run
```

### Accessing the Service

ZipServe runs as a web server and clients can request files to be zipped via a simple GET request from any web browser or HTTP client. The files should be specified as query parameters in the URL.

### Request Format

To request files to be zipped, use the following format in your web browser or HTTP client:

[http://localhost:8080/test1.txt&test2.txt&test3.txt](http://localhost:8080/test1.txt&test2.txt&test3.txt)

Replace `test1.txt`, `test2.txt`, `test3.txt` with the actual names of the files you wish to download. Files are separated with `&` symbol.

### Handling Missing Files

If any of the specified files do not exist on the server, the server will proceed to zip the remaining files that are available. If none of the requested files exist, a message indicating this will be displayed to the client, such as "No Files Requested"

## License

ZipServe is licensed under the GNU General Public License v3.0 (GPL-3.0). For more details, see the [LICENSE](https://github.com/jovanovicdima/zipserve/blob/master/LICENSE) file included with the source code or visit [GNU GPL v3.0 License](https://www.gnu.org/licenses/gpl-3.0.html).

This provides clear information about the licensing, ensuring that users are aware of the terms under which the software is distributed and used.

## Contact

Project Link: [https://github.com/jovanovicdima/zipserve](https://github.com/jovanovicdima/zipserve)
