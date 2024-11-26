// Libraries
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps612.h"

#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
#include "Wire.h"
#endif

#define OUTPUT_READABLE_QUATERNION

// Gyro
MPU6050 mpu;

#define INTERRUPT_PIN 2 

// MPU control/status vars
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

Quaternion q;

// ================================================================
// ===               INTERRUPT DETECTION ROUTINE                ===
// ================================================================

volatile bool mpuInterrupt = false;     // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
  mpuInterrupt = true;
}


// ================================================================
// ===                      INITIAL SETUP                       ===
// ================================================================

void setup_gyro() {
  // join I2C bus (I2Cdev library doesn't do this automatically)
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
  Wire.begin();
  Wire.setClock(400000); // 400kHz I2C clock. Comment this line if having compilation difficulties
#elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
  Fastwire::setup(400, true);
#endif

  // initialize serial communication
  // (115200 chosen because it is required for Teapot Demo output, but it's
  // really up to you depending on your project)

  while (!Serial); // wait for Leonardo enumeration, others continue immediately

  // NOTE: 8MHz or slower host processors, like the Teensy @ 3.3V or Arduino
  // Pro Mini running at 3.3V, cannot handle this baud rate reliably due to
  // the baud timing being too misaligned with processor ticks. You must use
  // 38400 or slower in these cases, or use some kind of external separate
  // crystal solution for the UART timer.

  // initialize device
  mpu.initialize();
  pinMode(INTERRUPT_PIN, INPUT);

  // load and configure the DMP
  devStatus = mpu.dmpInitialize();

  // supply your own gyro offsets here, scaled for min sensitivity
  mpu.setXGyroOffset(51);
  mpu.setYGyroOffset(8);
  mpu.setZGyroOffset(21);
  mpu.setXAccelOffset(1150);
  mpu.setYAccelOffset(-50);
  mpu.setZAccelOffset(1060);

  // make sure it worked (returns 0 if so)
  if (devStatus == 0) {
    // Calibration Time: generate offsets and calibrate our MPU6050
    mpu.CalibrateAccel(6);
    mpu.CalibrateGyro(6);
    // mpu.PrintActiveOffsets();

    // turn on the DMP, now that it's ready
    mpu.setDMPEnabled(true);

    // Digital low pass filter to try and filter out noise!
    mpu.setDLPFMode(MPU6050_DLPF_BW_10);

    // enable Arduino interrupt detection
    attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING);
    mpuIntStatus = mpu.getIntStatus();

    // set our DMP Ready flag so the main loop() function knows it's okay to use it
    // Serial.println(F("DMP ready! Waiting for first interrupt..."));
    dmpReady = true;

    // get expected DMP packet size for later comparison
    packetSize = mpu.dmpGetFIFOPacketSize();
  } else {
    // ERROR!
    // 1 = initial memory load failed
    // 2 = DMP configuration updates failed
    // (if it's going to break, usually the code will be 1)
    // Serial.print(F("DMP Initialization failed (code "));
    // Serial.print(devStatus);
  }
}


// Button state
int btnTop = 0;
int btnMid = 0;
int btnBot = 0;

int sheatheSwitchL = 0;
int sheatheSwitchR = 0;

// Pin numbers
#define HAPTIC_IN_PIN 5
#define BTN_TOP_PIN 10
#define BTN_MID_PIN 11
#define BTN_BOT_PIN 12

#define SHEATHE_L_PIN 7
#define SHEATHE_R_PIN 8
#define LED_PIN LED_BUILTIN


char inputBuffer;

void setup() {
  setup_gyro();
  
  Serial.begin(9600);

  pinMode(BTN_TOP_PIN, INPUT_PULLUP);
  pinMode(BTN_MID_PIN, INPUT_PULLUP);
  pinMode(BTN_BOT_PIN, INPUT_PULLUP);
  pinMode(SHEATHE_L_PIN, INPUT_PULLUP);
  pinMode(SHEATHE_R_PIN, INPUT_PULLUP);
  pinMode(LED_PIN, OUTPUT);
  pinMode(HAPTIC_IN_PIN, OUTPUT);
}


void writeFloat(float f){
  byte * b = (byte *) &f;
  Serial.write(b[0]);
  Serial.write(b[1]);
  Serial.write(b[2]);
  Serial.write(b[3]);
  return;
}

void sendState()
{
  char switches = 0;
  switches |= btnTop;
  switches |= btnMid << 1;
  switches |= btnBot << 2;
  switches |= sheatheSwitchL << 3;
  switches |= sheatheSwitchR << 4;
  Serial.print(switches);
  writeFloat(q.x);
  writeFloat(q.y);
  writeFloat(q.z);
  writeFloat(q.w);
}

void loop() {
  // Should be true when pressed, false when not pressed, to work properly with the game logic
  // Might need inversion for pullup switches
  btnTop = !digitalRead(BTN_TOP_PIN);
  btnMid = !digitalRead(BTN_MID_PIN);
  btnBot = !digitalRead(BTN_BOT_PIN);

  // Should be false when sheathe is in, true when sheathe is out, to work properly with game logic
  sheatheSwitchL = digitalRead(SHEATHE_L_PIN);
  sheatheSwitchR = digitalRead(SHEATHE_R_PIN);

  // TEMP TESTING BTNS ONLY, GET RID OF THIS LATER
  sheatheSwitchL = false;
  sheatheSwitchR = false;

  // read a packet from gyro FIFO
  if (mpu.dmpGetCurrentFIFOPacket(fifoBuffer)) {
    mpu.dmpGetQuaternion(&q, fifoBuffer);
  }

  // Wait for ack from game instance to send state to game instance
  if(Serial.available())
  {
    Serial.readBytes(&inputBuffer, 1);
    if(inputBuffer == (char)255)
    {
      sendState();
    }
  }

  // Vibrate when sword is drawn
  if(sheatheSwitchL && sheatheSwitchR)
  {
    digitalWrite(HAPTIC_IN_PIN, HIGH);
  }
  else
  {
    digitalWrite(HAPTIC_IN_PIN, LOW);
  }

  // LED for debugging btns
  if (btnTop || btnMid || btnBot) {
    digitalWrite(LED_PIN, HIGH);
  } else {
    digitalWrite(LED_PIN, LOW);
  }
}