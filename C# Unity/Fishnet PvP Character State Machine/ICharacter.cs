namespace Assets.App.Scripts.Characters
{
    public interface ICharacter
    {
        /// <summary>
        /// Called on owner and server on death
        /// </summary>
        public void Death();
        /// <summary>
        /// Called on owner and server on death
        /// </summary>
        public void Respawn();
    }
}