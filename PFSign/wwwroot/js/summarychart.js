function timestats(data, id) {
    var Stat = G2.Stat;
    var chart = new G2.Chart({
        id: id,
        forceFit: true,
        height: 450,
        plotCfg: {
            margin: [20, 60, 60, 60]
        }
    });
    chart.source(data, {
        'num': {
            alias: '签到次数'
        },
        'hour': {
            tickInterval: 2,
            alias: '时间段',
            formatter: function (val) {
                return val + '~' + (val + 1);
            }
        }
    });
    chart.interval().position('hour*num').color('#b0e0e6').label('num');
    chart.render();
}