/*
  FunctionalButtton.cpp - Library for fully implemented button.
  Note: If not checking one of the functions in Loop, you may have issues reading HoldDown.
  Created by All Parts Combined, 2021.
  Released into the public domain.
*/
#ifndef FunctionalButton_h
#define FunctionalButton_h

#include "Arduino.h"
#include "Debounce.h"

class FunctionalButton
{
public:
	FunctionalButton(int pin);
	boolean Down();
	boolean ClickUp();
	boolean HoldDown();
	boolean HoldUp();
	boolean Up();
	boolean Held();
	unsigned long buttonHoldDuration = 2000;
private:
	void Update();
	int previousButtonReading;
	Debounce button;
	unsigned long buttonPressedTime = 0;
	boolean down = false;
	boolean clickUp = false;
	boolean holdDown = false;
	boolean holdUp = false;
	boolean up = false;
	boolean held = false;

};

#endif