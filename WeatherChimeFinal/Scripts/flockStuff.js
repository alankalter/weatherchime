function roundedToFixed(_float, _digits) {
    var rounder = Math.pow(10, _digits);
    return parseFloat((Math.round(_float * rounder) / rounder).toFixed(_digits));
}

function mulFibGen(length) {
    var mulFib = [0, .01];
    for (i = 0; i < length; i++) {
        mulFib[i + 2] = roundedToFixed((mulFib[i] + mulFib[i + 1]), 2);
    }
    return mulFib;
}

//ranges
var tempRange = [-90, 57];
var humidityRange = [0, 100];
var pressureRange = [870, 1084];
var windSpeedRange = [0, 408];
var fundamentalRange = [110, 440];
var whiteNoiseFreqRange = [400, 20000];
var whiteNoiseMulFreq = [0.02, 0.1];
var mulFibGenValsRange = [0, 15];
var clickLengthRange = [0.1, 1];
var clockRange = [0, 2];


//scaled values
var pressureFundamentalScl = convertRange(pressureRange[0], pressureRange[1], fundamentalRange[0], fundamentalRange[1], values.Pressure);
var pressureClockScl = Math.ceil(convertRange(pressureRange[0], pressureRange[1], clockRange[0], clockRange[1], values.Pressure));
var windSpeedwhiteNoiseFreqScl = convertRange(windSpeedRange[0], windSpeedRange[1], whiteNoiseFreqRange[0], whiteNoiseFreqRange[1], values.Windspeed);
var windSpeedwhiteNoiseMulFreqScl = convertRange(windSpeedRange[0], windSpeedRange[1], whiteNoiseMulFreq[0], whiteNoiseMulFreq[1], values.Windspeed);
var tempmulFibGenValsLength = Math.ceil(convertRange(tempRange[0], tempRange[1], mulFibGenValsRange[0], mulFibGenValsRange[1], values.Temp));
var humidityClickLengthScl = convertRange(humidityRange[0], humidityRange[1], clickLengthRange[0], clickLengthRange[1], values.Humidity);

console.log("tmfgvl: " + tempmulFibGenValsLength);

var mulFibGenVals = mulFibGen(tempmulFibGenValsLength);

console.log(values);


var environment = flock.init();

var fundamental = pressureFundamentalScl / 2;

var polySynth = flock.synth.polyphonic({
    synthDef: {
        id: "carrier",
        ugen: "flock.ugen.sin",
        freq: fundamental,
        mul: {
            id: "env",
            ugen: "flock.ugen.asr",
            attack: 0.25,
            sustain: 1.0,
            release: 1.0
        }
    }
});

var score = [
    {
        action: "noteOn",
        noteName: "root",
        change: {
            "carrier.freq": fundamental
        }
    },
    {
        action: "noteOn",
        noteName: "two",
        change: {
            "carrier.freq": fundamental * 9 / 8
        }
    },

    {
        action: "noteOn",
        noteName: "mediant",
        change: {
            "carrier.freq": fundamental * 5 / 4
        }
    },

    {
        action: "noteOn",
        noteName: "four",
        change: {
            "carrier.freq": fundamental * 4 / 3
        }
    },

    {
        action: "noteOn",
        noteName: "dominant",
        change: {
            "carrier.freq": fundamental * 3 / 2
        }
    },
     {
         action: "noteOn",
         noteName: "six",
         change: {
             "carrier.freq": fundamental * 5 / 3
         }
     },
     {
         action: "noteOn",
         noteName: "seven",
         change: {
             "carrier.freq": fundamental * 15 / 8
         }
     },

    {
        action: "noteOff",
        noteName: "root"
    },
    {
        action: "noteOff",
        noteName: "two"
    },
    {
        action: "noteOff",
        noteName: "mediant"
    },
    {
        action: "noteOff",
        noteName: "four"
    },
    {
        action: "noteOff",
        noteName: "dominant"
    },
    {
        action: "noteOff",
        noteName: "six"
    },
    {
        action: "noteOff",
        noteName: "seven"
    }
];

var clock = flock.scheduler.async();
var clock2 = flock.scheduler.async();


var idx = 0;
clock.repeat(pressureClockScl, function () {
    if (idx >= score.length) {
        idx = 0;
    }

    idx = Math.ceil(score.length * Math.random());

    var event = score[idx];
    if (event != undefined) {
        polySynth[event.action](event.noteName, event.change);
    }
    
});

var clicksIdx = -2;
clock2.repeat(5, function () {
    if (clicksIdx = 1) {

        var clickDyn = mulFibGenVals[Math.ceil((tempmulFibGenValsLength + 1) * Math.random())];
        //console.log(clickDyn);
        clickSynth.set("clicky.end", clickDyn);

        clicksIdx = 0;

    }
    else {
        clickSynth.set("clicky.end", clickDyn);
        clicksIdx += 1;
    }
});


//whitenoise
var synth = flock.synth({
    synthDef: {
        ugen: "flock.ugen.filter.biquad.bp",
        source: {
            ugen: "flock.ugen.whiteNoise",
            mul: {
                id: "mod",
                ugen: "flock.ugen.sinOsc",
                //dyn
                freq: windSpeedwhiteNoiseMulFreqScl,
                mul: 0.20,

            }
        },
        //dyn
        freq: windSpeedwhiteNoiseFreqScl
        ,
        q: 0.5
    }
});

//clicks
var clickSynth = flock.synth({
    synthDef: {
        ugen: "flock.ugen.decay",
        source: {
            ugen: "flock.ugen.impulse",
            rate: "audio",
            freq: {
                id: "clicky",
                ugen: "flock.ugen.xLine",
                rate: "control",
                end: 0
            },
            phase: 0.25,
            mul: 0.2
        },
        time: humidityClickLengthScl,
        mul: {
            ugen: "flock.ugen.whiteNoise"
        }
    }
});


environment.start();
