#include <Servo.h>
#include <FS.h>

//Mode determines the behavior of the hand. 
enum Mode
{
  Off, Live, Playback
};

//Self explanatory.
enum Finger
{
  None, Thumb, Index, Middle, Ring, Pinky
};

//When a Message type is set, it determines how we will handle new incoming data.
enum MessageType
{
  Unspecified, ModeChange, FingerData, SetFile
};

Servo thumbServo;
Servo indexServo;
Servo middleServo;
Servo ringServo;
Servo pinkyServo;

uint8_t thumbMax = 125;
uint8_t indexMax = 150;
uint8_t middleMax = 165   ;
uint8_t ringMax = 165;
uint8_t pinkyMax = 180;

uint8_t thumbPosition;
uint8_t indexPosition;
uint8_t middlePosition;
uint8_t ringPosition;
uint8_t pinkyPosition;

Mode mode = Playback; //Start in playback so the hand can be used without the PC app.
Finger currentFinger = None;
MessageType messageType = Unspecified;

const byte numChars = 64;
char receivedChars[numChars];

boolean recording = false;
boolean newData = false;
boolean debugBytes = false;

int dataIndex = 0;

int fileIndex = 0;
int maxNumFiles = 5;
int fileOffset;

//Initialize Servos, SPIFFS, Serial.
void setup()
{
  SPIFFS.begin();
  //Servos are on GPIO pins 0,2,4,5,16 of NodeMCU ESP8266
  thumbServo.attach(0);
  indexServo.attach(16);
  middleServo.attach(2);
  ringServo.attach(5);
  pinkyServo.attach(4);
  Serial.begin(9600); // opens serial port, sets data rate to 9600 bps
  fileIndex = GetFileIndex();
}

void loop()
{
  if (mode == Off)
  {
    //Open Hand When Off
    SetPosition(Thumb, 0);
    SetPosition(Index, 0);
    SetPosition(Middle, 0);
    SetPosition(Ring, 0);
    SetPosition(Pinky, 0);
  }
  else if (mode == Playback)
  {
    //Playback mode has it's own loop.
    PlaybackLoop();
  }
  WriteServos();
  ReceiveSerial();
  if (!newData)
    return;
  //showNewData(); //Uncomment to print all received data.
  newData = false;
  ProcessNewData();
}

//Set the servos to their saved Positions
void WriteServos()
{
  thumbServo.write(thumbPosition);
  indexServo.write(indexPosition);
  middleServo.write(middlePosition);
  ringServo.write(ringPosition);
  pinkyServo.write(pinkyPosition);
}

//Handle incoming Serial data
void ProcessNewData()
{
  dataIndex = 0;
  while (receivedChars[dataIndex] != '\0')
  {
    if (!ProcessGeneralData()) //Only process Live data if we didn't consume a message in the in General Data (which affects all modes)
    {
      if (mode == Live)
      {
        ProcessLiveData();
      }
    }
    dataIndex++;
  }
}

//Process Data which may affect any mode. 
boolean ProcessGeneralData()
{
  //Special Messages:
  //250 = Set Recording File
  //251 = Start Recording
  //252 = Stop Recording
  //253 = Start Receiving Finger Data
  //254 = Stop Receiving Finger Data
  //255 = Mode Change
  boolean consumedMessage = false;
  switch (messageType)
  {
    case Unspecified: //Currently no specific message type is set
      if (receivedChars[dataIndex] == (char)255) //We are getting a set of changes to change the mode
      {
        messageType = ModeChange;
        consumedMessage = true;
      }
      else if (receivedChars[dataIndex] == (char)253) //We are getting a finger data set of messages
      {
        messageType = FingerData;
        RecordDataStart();
        consumedMessage = true;
      }
      else if (receivedChars[dataIndex] == (char)250) //We are getting a set of messages to change the current recording/playback file
      {
        messageType = SetFile;
        consumedMessage = true;
      }
      else if (mode == Live)
      {
        if (receivedChars[dataIndex] == (char)251) //We got a message to Start recording 
        {
          OverwriteFile(GetFilename(), "");
          recording = true;
          consumedMessage = true;
        }
        else if (receivedChars[dataIndex] == (char)252) //We got a message to Stop recording
        {
          recording = false;
          consumedMessage = true;
        }
      }
      break;
    case ModeChange: //We got a Mode Change message, so the next message will specify the new Mode. 
      if (receivedChars[dataIndex] == (char)1)
      {
        SetMode(Off);
        consumedMessage = true;
      }
      if (receivedChars[dataIndex] == (char)2)
      {
        SetMode(Live);
        consumedMessage = true;
      }
      else if (receivedChars[dataIndex] == (char)3)
      {
        SetMode(Playback);
        fileOffset = 0;
        consumedMessage = true;
      }
      messageType = Unspecified; //Reset the current message type. 
      break;
    case FingerData: //We got a Finger Data message, so until we get another message signifying the Finger Data is finished, we will process all messages as Finger Data. 
      if (receivedChars[dataIndex] == (char)254)
      {
        messageType = Unspecified;
        consumedMessage = true;
      }
      break;
    case SetFile: //We got a Set File message so the next message will tell us what File to use. 
      SetFileIndex((int)receivedChars[dataIndex] - 1);
      messageType = Unspecified;
      consumedMessage = true;
      break;
  }
  return consumedMessage;
}

//Process Data for live mode - specifically, FingerData messages. 
void ProcessLiveData()
{
  if (messageType != FingerData)
    return;
  if (currentFinger == None)
  {
    //If no finger is specified, the next message will tell us what it is. 
    if (receivedChars[dataIndex] == 'T')
      currentFinger = Thumb;
    else if (receivedChars[dataIndex] == 'I')
      currentFinger = Index;
    else if (receivedChars[dataIndex] == 'M')
      currentFinger = Middle;
    else if (receivedChars[dataIndex] == 'R')
      currentFinger = Ring;
    else if (receivedChars[dataIndex] == 'P')
      currentFinger = Pinky;
  }
  else
  {
    //If a finger is specified, the next message will tell us the amount it is closed. 
    SetPosition(currentFinger, (uint8_t)receivedChars[dataIndex] - 1);
    currentFinger = None;
  }
}

//Get the current recording file index from another file saved in memory (so it can persist when the hand is turned off)
int GetFileIndex()
{
  return ReadFile("/fileIndex.txt").toInt();
}

//Set the file index and write it to a file. 
void SetFileIndex(int ind)
{
  fileIndex = ind;
  fileOffset = 0;
  String indString = "";
  indString += ind;
  indString += "\n";
  OverwriteFile("/fileIndex.txt", indString);
}

//Set the mode.
void SetMode(Mode newMode)
{
  mode = newMode;
  Serial.print("Setting mode to ");
  Serial.println(newMode);
}

//Figure out and set the position of a specified finger to a specified percentage closed. Also, record it if Recording is active. 
void SetPosition(Finger type, uint8_t amountClosed)
{
  switch (type)
  {
    case Thumb:
      thumbPosition = GetAngle(amountClosed, thumbMax);
      RecordPosition(type, thumbPosition);
      break;
    case Index: // Set Index Position
      indexPosition = GetAngle(amountClosed, indexMax);
      RecordPosition(type, indexPosition);
      break;
    case Middle: // Set Middle Position
      middlePosition = GetAngle(amountClosed, middleMax);
      RecordPosition(type, middlePosition);
      break;
    case Ring: // Set Ring Position
      ringPosition = GetAngle(amountClosed, ringMax);
      RecordPosition(type, ringPosition);
      break;
    case Pinky: // Set Pinky Position
      pinkyPosition = GetAngle(amountClosed, pinkyMax);
      RecordPosition(type, pinkyPosition);
      break;
  }
}

//Get the filename string for the current file index.
String GetFilename()
{
  String filename = "/file";
  filename += fileIndex + ".txt";
  return filename;
}

//Record a '-' to indicate that a set of finger data messages has started.  This is used to delay the proper amount between received messages in playback.
void RecordDataStart()
{
  if (!recording)
    return;
  String contents = "-\n";
  AppendFile(GetFilename(), contents);
}

//Record the position of a finger to a file. 
void RecordPosition(Finger type, uint8_t fingerPosition)
{
  if (!recording)
    return;
  String contents = "";
  contents += type;
  contents += " ";
  contents += fingerPosition;
  contents += "\n";
  AppendFile(GetFilename(), contents);
}

//Handle playback by reading a file and using its contents to set finger positions. 
void PlaybackLoop()
{
  //First, we read the full file and check its length so we know how many bytes we can possibly read. 
  String fullString = ReadFile(GetFilename());
  int totalLength = fullString.length();
  //Next we open the file to read the current line
  File f = SPIFFS.open(GetFilename(), "r");
  if (!f)
    return;
  f.seek(fileOffset, SeekSet);  //Navigate to the saved position in the file: 
  String line = f.readStringUntil('\n'); //Read the next line
  fileOffset = (fileOffset + (line.length() + 1)) % totalLength; //Adjust the offset, wrapping back to the beginning of the file if needed
  while (!line.equals("-") && line.length() > 0) //Loop through all saved data until we either get an empty line or a '-'
  {
    //Parse the finger and position bits from the line, then set the finger positions based on them
    int index = line.indexOf(' ');
    String finger = line.substring(0, index);
    String pos = line.substring(index + 1);
    int fingerInt = finger.toInt();
    int posInt = pos.toInt();
    switch (fingerInt)
    {
      case 1: //Thumb
        thumbPosition = posInt;
        break;
      case 2: //Index
        indexPosition = posInt;
        break;
      case 3: //Middle
        middlePosition = posInt;
        break;
      case 4: //Ring
        ringPosition = posInt;
        break;
      case 5: //Pinky
        pinkyPosition = posInt;
        break;
    }
    line = f.readStringUntil('\n');
        fileOffset = (fileOffset + (line.length() + 1)) % totalLength; //Adjust the offset, wrapping back to the beginning of the file if needed
  }
  f.close();
  delay(50); //Delay to properly set the speed of the recorded motion. Data is received over serial at 50ms, so each "-" signifies a 50ms pause. 
}

//Calculate the servo angle based on how closed it should be and what angle constitutes 100% closed. 
int GetAngle(uint8_t amountClosed, uint8_t maxValue)
{
  float percentClosed = ((float)amountClosed) / (float)128;
  return (int)(percentClosed * (float)maxValue);
}

//Receive new serial data
void ReceiveSerial()
{
  static boolean recvInProgress = false;
  static byte ndx = 0;
  char rc;
  while (Serial.available() > 0 && newData == false)
  {
    rc = Serial.read();
    receivedChars[ndx] = rc;
    ndx++;
    if (ndx >= numChars)
    {
      ndx = numChars - 1;
    }
  }
  if (ndx > 0)
  {
    receivedChars[ndx] = '\0'; // terminate the string
    if (debugBytes)
    {
      for (int i = 0; i < ndx; i++)
      {
        Serial.println((uint8_t)receivedChars[i], DEC);
      }
    }
    ndx = 0;
    newData = true;
  }
}

//Print serial data to console as a string.
void showNewData()
{
  if (newData == true) {
    Serial.print("Arduino Received Data: ");
    Serial.println(receivedChars);
    newData = false;
  }
}

//Read an entire file to a string.
String ReadFile(String filename)
{
  File f = SPIFFS.open(filename, "r");
  if (!f)
  {
    f.close();
    return "FAILURE";
  }
  String contents = f.readString();
  f.close();
  return contents;
}

//Overwrite a file with a specified string.
void OverwriteFile(String filename, String contents)
{
  File f = SPIFFS.open(filename, "w");
  if (!f)
  {
    f.close();
    return;
  }
  f.print(contents);
  f.close();
}

//Add a string to the end of a file.
void AppendFile(String filename, String contents)
{
  File f = SPIFFS.open(filename, "a");
  if (!f)
  {
    f.close();
    return;
  }
  f.print(contents);
  f.close();
}
