/*
  Debounce.cpp - Library for Debouncing a button.
  Note: Read should be called in Loop.
  Created by All Parts Combined, 2021.
  Released into the public domain.
*/
#ifndef Debounce_h
#define Debounce_h

#include "Arduino.h"

class Debounce
{
public:
	Debounce(int pin);
	int Read();
	unsigned long delay = 50;
private:
	int _pin;
	unsigned long lastDebounceTime = 0;
	int lastState = LOW;
	int state = LOW;
};

#endif