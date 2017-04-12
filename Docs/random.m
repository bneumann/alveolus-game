function ret = random(type, mu, sigma, varargin)
  ret = normrnd(mu, sigma, varargin{:});
end