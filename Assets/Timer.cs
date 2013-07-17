using UnityEngine;
using System.Collections;

public class Timer {
	private System.Diagnostics.Stopwatch _stopwatch;
	
	public Timer(){
		_stopwatch = new System.Diagnostics.Stopwatch();
	}
	
	public void Start(){
		
        _stopwatch.Start ();
	}
	
	public void Stop(){
        _stopwatch.Stop ();
	}
	
	public double Elapsed(){
		return _stopwatch.Elapsed.TotalMilliseconds;
	}
}

public class ScopedTimer {
	private System.Diagnostics.Stopwatch _stopwatch;
	private string _title;	
	
	public ScopedTimer(string title){
		_title = title;
		_stopwatch = new System.Diagnostics.Stopwatch();
        _stopwatch.Start ();
	}
	
	~ScopedTimer(){
        _stopwatch.Stop ();
        System.TimeSpan ts = _stopwatch.Elapsed;
       	Debug.Log (_title + " Millis: " + ts.Ticks + " Ticks: " + ts.Milliseconds); 
	}
	
}