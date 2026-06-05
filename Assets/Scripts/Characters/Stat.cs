using System.Collections.Generic;
using System.Linq;

public class Stat
{
    public float BaseValue;

    private List<float> _flatModifiers = new();
    private List<float> _percentModifiers = new();


    public float Value
    {
        get
        {
            float flat = BaseValue + _flatModifiers.Sum();
            return flat * (1f + _percentModifiers.Sum());
        }
    }

    public void AddFlat(float ammount) => _flatModifiers.Add(ammount);
    public void RemoveFlat(float ammount) => _flatModifiers.Remove(ammount);
    public void AddPercent(float amount) => _percentModifiers.Add(amount);
    public void RemovePercent(float amount) => _percentModifiers.Remove(amount);

}