#include <string>
#include <iostream>
#include <SFML/Graphics.hpp>
using namespace std;

class Button
{
private:
	sf::RectangleShape button;
	sf::Text buttonText;
	
public:
	Button(sf::Vector2f position, sf::Color color, sf::Font& font, string text)
	{
		button.setPosition(position);
		button.setFillColor(color);
		button.setSize(sf::Vector2f(100.0f, 30.0f));
		button.setOutlineColor(sf::Color::Red);
		button.setOutlineThickness(1.0f);
		
		buttonText.setFont(font);
		buttonText.setString(text);
		buttonText.setPosition(position + sf::Vector2f(20.0f, 5.0f));
		buttonText.setCharacterSize(16);
		buttonText.setColor(sf::Color::Red);
	}
	~Button() {}
	bool mouseOver(int x, int y)
	{
		sf::Vector2f buttonPosition = button.getPosition();
		if (x > buttonPosition.x && x < buttonPosition.x + button.getSize().x &&
			y > buttonPosition.y && y < buttonPosition.y + button.getSize().y)
		{
			return true;
		}
		return false;
	}
	void click()
	{
		cout << "Button has been clicked" << endl;
	}
	void render(sf::RenderWindow& window)
	{
		window.draw(button);
		window.draw(buttonText);
	}
};

int main()
{
	sf::RenderWindow window(sf::VideoMode(640, 480), "Buttons");
	window.setFramerateLimit(30);
	
	sf::Font font;
	if (!font.loadFromFile("arial.ttf")) return 1;
	
	Button button(sf::Vector2f(200.0f, 300.0f), sf::Color::White, font, "Click me");
	
	while (window.isOpen())
	{
		sf::Event ev;
		while (window.pollEvent(ev))
		{
			if (ev.type == sf::Event::Closed) window.close();
			else if (ev.type == sf::Event::MouseButtonPressed)
			{
				if (button.mouseOver(ev.mouseButton.x, ev.mouseButton.y))
				{
					button.click();
				}
			}
		}
		
		window.clear(sf::Color::Black);
		button.render(window);
		window.display();
	}
	
	return 0;
}