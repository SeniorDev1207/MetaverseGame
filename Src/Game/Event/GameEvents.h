#ifndef EVENT_GAMEEVENTS_H
#define EVENT_GAMEEVENTS_H

#include <boost/foreach.hpp>
#include "Base/Marcos.h"

namespace Egametang {

enum
{
	SPELL_START     = 0,
	SPELL_FINISH    = 1,
	ADD_BUFF        = 2,
	REMOVE_BUFF     = 3,
};

class GameEvents
{
private:
	std::vector<std::list<Event> > events;

public:
	GameEvents();

	void AddEvent(Event& event);

	void Excute(int type, ContexIf* contex);
};

} // namespace Egametang

#endif // EVENT_GAMEEVENTS_H
