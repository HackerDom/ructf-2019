# WeaherD
The service consists of two parts rust service with open source code and black box service.

Each command have it's own insatance of rust service, and there is only one shared instance of NotificationAPI.

Users can create channels and subscribe to the channel with tokens provided by black box service. Token generation alogrythm is unknown.

Messages 

## Attack 1

Users can send custom requests to NotificationAPI and analyze token generation algorythm.
Tokens are generation takes name and timestamp from dto and generate UUID based on timestamp and first 8 bytes of source_name via algorythm:
`
 public static string Generate(NotificationApiRequest request)
    {
      var timeBytes = BitConverter.GetBytes(request.timestamp/60);
      var part1 = BitConverter.ToInt32(new[] { timeBytes[0], timeBytes[1], timeBytes[2], timeBytes[3] });
      var part2 = BitConverter.ToInt16(new[] { timeBytes[4], timeBytes[5] });
      var part3 = BitConverter.ToInt16(new[] { timeBytes[6], timeBytes[7] });
      var srcBytes = Encoding.UTF8.GetBytes(request.source);
      var bytes = new byte[8];
      for(var i = 0; i < Math.Min(srcBytes.Length, 8); i++)
        bytes[i] = srcBytes[i];
      var guid = new Guid(part1, part2, part3, bytes 
`
Every request with same timestamp with precision of a minute and same 8 bytes of string representation of have same tokens.

Users can discover new channel created by check system by sending get all sources request every minute and monitoring new sources.

### Defense:
By default all timestamp is current time, but NotificationAPI accept any timestamps, if some random number is used instead of current time as timestamp for token generation token would be different.


## Attack 2

There is unescaped fieild race in create source dto. Users could inject custom xml code for example

'race' : '</text><image xlink:href=\"some_other_prerendered_png.png\" x=\"0\" y=\"0\" height=\"640px\" width=\"480px\"/><text>smth'

and steal another picture.

### Defense:
Escape all every strings inserted in svg
