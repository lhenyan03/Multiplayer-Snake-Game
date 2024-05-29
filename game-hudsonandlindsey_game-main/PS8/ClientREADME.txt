Authors: Hudson Bowman and Lindsey Henyan
Created: November 27, 2023

# Summary
	11/27/2023:
		For our snake client game we decided to use all of the provided images, and draw all of the other game objects.  
		To make the code more simplistic we created a GameObject Abstract class that the Snake, Wall, and Powerup all 
		inherit.  We follow the practice of separation of concerns with different projects that represent the model (Model)
		controller (GameController), and view (Snake Client). Our Death Animation consists of the snake dissolving into 
		many red balls that bounce around in the impact area. The colors we used to represent different snakes are  gold, 
		blue, indigo, green, medium purple, pale violet red, misty rose, and yellow. All bugs that have been discovered 
		have been resolved. We faced a cosmetic problem of not being able to load images beyond the ones that were provided 
		for us. We tried many different methods to try and load and draw them but noting worked.

#Outside Resources
	11/16/2023:
		Processing Key Presses in C#
			https://stackoverflow.com/questions/4255675/processing-key-presses-in-c-sharp
