Overview:
This application is a time-boxed code challenge to implement a server application that can receive 9-digit numbers (or a termination signal) from up to 5 client applications via a TCP socket.

Build Process:
To build this application you will need Visual Studio 2017 and the .Net Framework 4.6.1 installed as well as an internet connection with access to NuGet.org. 
To compile the project, open the NumbersServerCodeChallenge.sln file in Visual Studio and build the solution. Building the entire solution will compile all 3 projects in the 
solution which include: 
1. NumbersServer - this is the main application
2. NumbersServerTests - this is the test library for the NumbersServer application
3. SampleNumbersClient - this is a very basic implementation of a client application that can be used to test or demonstrate the NumbersServer application

Building the entire solution will generate the executable code for all 3 projects and place them in either [project_folder]\bin\debug or [project_folder]\bin\release depending on the 
type of build requested (debug or release respectively).

Running the application:
Open a command prompt window and navigate to the NumbersServer\bin\Debug (or release) folder and run the command NumbersServer.exe without any arguments. The application can be terminated 
at any point by using Ctrl+c in the command prompt window. The application will listen for that event and gracefully shutdown the application.

Running the sample client application:
Open a command prompt window and navigate to the SampleNumbersClient\bin\Debug (or release) folder and run the command SampleNumbersClient.exe without any arguments. The application can be terminated 
at any point by using Ctrl+c in the command prompt window. The application will listen for that event and gracefully shutdown the application.

Running the automated tests:
The tests are not run automatically as part of the build process, but can be run manually either through Visual Studio or an NUnit test runner.

Code structure and data flow:
The main application consists of 6 classes.
1. Program - This is the entry point for the application and it constructs most of the shared resources for the application, as well as the thread that outputs some statistics to the console.
2. SocketServer - This is the class that contains all of the code for interacting with the socket.
3. MessageListener - This class is responsible for parsing and validating the raw message received from the SocketServer.
4. DuplicateNumberTracker - This class encapsulates the technique used to track duplicate numbers. The current implementation uses the .Net BitArray to implement a bitvector.
5. Report - This class tracks the data necessary to generate the period reports as well as generating the string representation of that report.
6. Logger - This class creates/truncates the log file as well as performs all of the writes to that file.

When a client sends a number through the socket to the application it goes through the following data flow.
1. The SocketServer receives the message
2. The SocketServer passes the raw message to the MessageListener
3. The MessageListener asks the DuplicateNumberTracker to track the number
4. The DuplicateNumberTracker reports back to the MessageListener if the number was a duplicate or not
5. a. If the number was not a duplicate the MessageListener notifies the Report and Logger about the unique number
5. b. If the number was a duplicate the MessageListener notifies the Report about the duplicate

Other technical decisions:
-A semaphore was used to control the number of active sockets
-Monitors (via the lock keyword) were used to synchronize access to the report and bitvector data
-A combination of a separate background thread and sleep command are used to periodically report the statics to standard out

Assumptions:
-A simple commmand line application is satisfactory, as opposed to creating a windows service.
-Getting at least 2M numbers processed per 10s reporting period is satisfactory for the time-boxed challenge.
-It is ok to target .Net Framework version 4.6.1
-Visual Studio 2017 with access to NuGet.org is available
-The ability to read from the log file while the application is running is desired
-There is adequate memory available in the system to accomodate a bitvector for tracking duplicate values (~125MB for the bitvector)
-The log file should be written to the "current" directory

Known Issues:
1. Client applications that establish a connection but never send any data or stop sending data without closing the connection, can lead to a denial of service attack.

Ideas for the future:
-Address the denial of service issue by exploring the following
	-Use the synchronous socket api and leverage time outs
	-Continue to use the asynchronous socket api and leverage a timer thread to close idle/inactive connections
-Explore batching up writes to the log file
-Use a profiler to identify slow parts of the code base
-Consider using a different bitvector implementation to handle duplicate number detection
-Add a method to build the project from the command line.
-Add some end-to-end tests that execute the entire application
-Simple refactorings such as resharper recommendations, and renaming some of the classes to better clarify their intent.
-Log exceptions instead of just swallowing them
