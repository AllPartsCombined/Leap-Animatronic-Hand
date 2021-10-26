/*
  Debounce.cpp - Library for Debouncing a button.
  Note: Read should be called in Loop.
  Created by All Parts Combined, 2021.
  Released into the public domain.
*/

#include "Arduino.h"
#include "Debounce.h"

Debounce::Debounce(int pin)
	: _pin(pin)
{
}


int Debounce::Read()
{
	int currentState = digitalRead(_pin);


	if (currentState != lastState) 
		lastDebounceTime = millis();
	

	if ((millis() - lastDebounceTime) > delay) 
	{
			state = currentState;
	}

	lastState = currentState;
	return state;
}