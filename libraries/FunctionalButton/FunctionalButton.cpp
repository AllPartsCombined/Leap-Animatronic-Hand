/*
  FunctionalButtton.cpp - Library for fully implemented button.
  Note: If not checking one of the functions in Loop, you may have issues reading HoldDown.
  Created by All Parts Combined, 2021.
  Released into the public domain.
*/

#include "Arduino.h"
#include "FunctionalButton.h"

FunctionalButton::FunctionalButton(int pin)
	: button(pin)
{
}


void FunctionalButton::Update()
{
	down = false;
	clickUp = false;
	holdDown = false;
	holdUp = false;
	up = false;

	int reading = button.Read();

	if (reading != previousButtonReading)
	{
		if (reading == HIGH)
		{
			buttonPressedTime = millis();
			down = true;
		}
		else
		{
			up = true;
			if (held)
			{
				held = false;
				holdUp = true;
			}
			else
			{
				clickUp = true;
			}
		}
	}

	if (!held && reading == HIGH && (millis() - buttonPressedTime) > buttonHoldDuration)
	{
		//Button held
		held = true;
		holdDown = true;
	}

	previousButtonReading = reading;
}

boolean FunctionalButton::Down()
{
	Update();
	return down;
}

boolean FunctionalButton::ClickUp()
{
	Update();
	return clickUp;
}

boolean FunctionalButton::HoldDown()
{
	Update();
	return holdDown;
}

boolean FunctionalButton::HoldUp()
{
	Update();
	return holdUp;
}

boolean FunctionalButton::Up()
{
	Update();
	return up;
}

boolean FunctionalButton::Held()
{
	Update();
	return held;
}