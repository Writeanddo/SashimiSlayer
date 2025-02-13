// Libraries
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

// Button state
int btnTop = 0;
int btnMid = 0;
int btnBot = 0;

int sheatheSwitchL = 0;
int sheatheSwitchR = 0;

// Pin numbers
#define HAPTIC_IN_PIN 5
#define BTN_TOP_PIN 14
#define BTN_MID_PIN 15
#define BTN_BOT_PIN 16

#define SHEATHE_L_PIN 7
#define SHEATHE_R_PIN 8
#define LED_PIN 17

char inputBuffer;

Adafruit_BNO055 bno = Adafruit_BNO055(55);

void setup_gyro()
{
  /* Initialise the sensor */
  if(!bno.begin())
  {
    /* There was a problem detecting the BNO055 ... check your connections */
    Serial.print("Ooops, no BNO055 detected ... Check your wiring or I2C ADDR!");
    while(1);
  }
  
  delay(1000);
    
  bno.setExtCrystalUse(true);
}

void setup() {  
  Serial.begin(9600);

  setup_gyro();

  // pinMode(BTN_TOP_PIN, INPUT_PULLUP);
  // pinMode(BTN_MID_PIN, INPUT_PULLUP);
  // pinMode(BTN_BOT_PIN, INPUT_PULLUP);
  // pinMode(SHEATHE_L_PIN, INPUT_PULLUP);
  // pinMode(SHEATHE_R_PIN, INPUT_PULLUP);
  // pinMode(LED_PIN, OUTPUT);
  // pinMode(HAPTIC_IN_PIN, OUTPUT);
}


void writeFloat(float f){
  byte * b = (byte *) &f;
  Serial.write(b[0]);
  Serial.write(b[1]);
  Serial.write(b[2]);
  Serial.write(b[3]);
  return;
}

void sendState(imu::Quaternion q)
{
  char switches = 0;
  switches |= btnTop;
  switches |= btnMid << 1;
  switches |= btnBot << 2;
  switches |= sheatheSwitchL << 3;
  switches |= sheatheSwitchR << 4;
  Serial.print(switches);
  writeFloat(q.x());
  writeFloat(q.y());
  writeFloat(q.z());
  writeFloat(q.w());
}

void loop() {
  // Should be true when pressed, false when not pressed, to work properly with the game logic
  // Might need inversion for pullup switches
  // btnTop = !digitalRead(BTN_TOP_PIN);
  // btnMid = !digitalRead(BTN_MID_PIN);
  // btnBot = !digitalRead(BTN_BOT_PIN);

  // Should be false when sheathe is in, true when sheathe is out, to work properly with game logic
  // sheatheSwitchL = digitalRead(SHEATHE_L_PIN);
  // sheatheSwitchR = digitalRead(SHEATHE_R_PIN);

  // Wait for ack from game instance to send state to game instance
  if(Serial.available())
  {
    Serial.readBytes(&inputBuffer, 1);
    if(inputBuffer == (char)255)
    {
      imu::Quaternion quat = bno.getQuat();
      
      sendState(quat);
    }
  }

  // Serial.println(sheatheSwitchL);

  // Vibrate when sword is drawn
  // if(sheatheSwitchL && sheatheSwitchR)
  // {
  //   digitalWrite(HAPTIC_IN_PIN, HIGH);
  // }
  // else
  // {
  //   digitalWrite(HAPTIC_IN_PIN, LOW);
  // }

  // LED for debugging btns
  // if (btnTop || btnMid || btnBot) {
  //   digitalWrite(LED_PIN, HIGH);
  // } else {
  //   digitalWrite(LED_PIN, LOW);
  // }
}