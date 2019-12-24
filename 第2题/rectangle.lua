--假设小正方形与大矩形的边平行。
--没有这个假设的情况，我不能保证这是最佳方案，也不能保证它对于100以外的个数有推广意义。
--（比如m=n=1,装5、10、11个的时候用的是非对齐, https://www2.stetson.edu/~efriedma/squinsqu/）

function fillerAligned(m, n)
    local count = 100;
    local upper = math.max(m,n);
    local lower = 0;

    local error = 0.000001;

    while (math.abs(upper - lower) > error) do
        midSide = (upper + lower) / 2;
        midCount = math.floor(m / midSide) * math.floor(n / midSide);

        if (midCount >= count) then
            lower = midSide;
        else
            upper = midSide;
        end
        --print(math.min(m / math.floor(m / lower), n / math.floor(n / lower)))
    end

    return math.min(m / math.floor(m / lower), n / math.floor(n / lower));
end

--print(0.01)
print(fillerAligned(1,1));
print(fillerAligned(2,2));
print(fillerAligned(1,5));
print(fillerAligned(1,20));