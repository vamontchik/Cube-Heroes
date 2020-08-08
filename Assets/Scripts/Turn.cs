public class Turn
{
    private int turnCount;
    private int amountOfPlayers;

    public Turn(int amountOfPlayers)
    {
        this.turnCount = 0;
        this.amountOfPlayers = amountOfPlayers;
    }

    public int NextTurn()
    {
        int oldCount = turnCount;
        turnCount = (turnCount + 1) % amountOfPlayers;
        return oldCount;
    }
}
