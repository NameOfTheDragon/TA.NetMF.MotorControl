# Netduino Object Oriented Motor Control #

This project implements various types of motor control for Microsoft .NET Micro Framework in C#. The primary use case is the control of motors using Netduino hardware and popular widely available motor shields. Support for other platforms/shields and motor types may be added in future.

### What is this repository for? ###

Motors are objects, too! There is a lot of micro-framework code out there that looks like it was written in Fortran in the 1960s. We have tried to take a more object oriented approach to this library.

Some features and aims of this library:


- **Object oriented**: we strive to give each class a single responsibility and basically follow the SOLID best practices.
- **Clean code**: we strive to maintain 'clean code' that is well documented, readable by human beings and easy to maintain.
- **Asynchronous non-blocking**: Where it makes sense, we try to make operations autonomous and non-blocking. For example, when commanding a stepper motor to move to a certain position, the motor runs autonomously under control of a timer without any further interaction from user code. The acceleration and deceleration happens automatically and an event is fired when the motor stops. No 'call this method repeatedly from your main loop' type of requirements.
- **General purpose**: where possible, we have tried to avoid hard-coding things like hardware addresses. Where possible, dependencies are passed into the constructor, so that for example, multiple motors can be controlled simply by newing up multiple instances of the classes.
- **Community involvement welcome**: We would love to get some pull requests!
- **Permissive license**: All code in this repository is covered by the [MIT license](http://opensource.org/licenses/MIT "The MIT License"), which is a permissive license. In summary, anyone can do anything at all with this code with proper attribution and with no warranty. Commercial use is specifically allowed.

### How do I get set up? ###

The projects are configured for Netduino Plus 2 hardware and .net MicroFramework 4.3. There are [instructions on setting up Visual Studio 2012](http://forums.netduino.com/index.php?/topic/10479-netduino-plus-2-firmware-v431/ "How to set up the Netduiono SDK in Visual Studio 2012") on the Netduino forums. You should be able to clone the code, build it in Visual Studio 2012 or later and deploy it to your Netduino Plus.

Note: Netduino support for Visual Studio 2013 is experimental, but we've found it works brilliantly. If you are using VS2013, there are [special instructions](http://forums.netduino.com/index.php?/topic/10201-experimental-visual-studio-2013-support/ "How to set up the Netduino SDK in Visual Studio 2013") on setting up.

## Get the Binaries ##

If you just want to get going quickly with motor control and you don't need to build the source, then we have provided a NuGet package. Support for NuGet is built into Visual Studio, just search for "Motor Control" and you should find it.

### Contribution guidelines ###

We invite and encourage pull requests. Each request will undergo code review before being merged. We use [GitFlow](http://nvie.com/posts/a-successful-git-branching-model/ "a successful Git branching model"), which reserves the *master* branch for published releases. Therefore, please push your code to *develop* or a feature branch off develop. [Atlassian SourceTree](http://www.sourcetreeapp.com/ "Free Git and Mercurial client for Windows and Mac") has built-in support for GitFlow and makes it all very simple. 

When you push code or submit a pull request to this repository, you are agreeing that your code is irrevocably donated to the project under the project's [MIT License](http://opensource.org/licenses/MIT "The MIT License"). Note that the license allows for commercial use of the code. Please don't submit code if you are not comfortable with that.

### What needs to be done? ###

Some ideas for future work:

- Additional motor types
	- DC motors
	- Servo motors
- Other motor shields
	- Sparkfun motor control shield (part complete - stepper motor implemented)
	- Adafruit motor shiled V1 - abandoned due to the product becoming obsolete.
	- Adafruit motor shield V2 - part complete (stepper motor implemented).
- Unit tests: We love unit tests, but haven't found a good way of doing that with micro-framework projects. We would love some contribution in this area. Can you work out how we could unit test a micro-framework project?
- Make the code cleaner - we try to write clean code but there is always room for improvement.
- Remove any SOLID violations. Again, always room for improvement.

### Who do I talk to? ###

* Repo owner/admin: Tim Long ([Tigra Astronomy](http://tigra-astronomy.com/ "Tigra Astronomy"))

-----
This project is licensed under the [MIT license](http://opensource.org/licenses/MIT "The MIT License")