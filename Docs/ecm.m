%Epithelial cell model
function dfx = ecm(~,x,N,cel,pars, matBac)
global contador
contador = contador + 1; %Count the iterations
%Parameters
x(x < 1e-6) = 0;
R           = cel*N; %Total length of surface, microm
dx          = R/N; %Thickness of each layer microm
NN          = N*N; %Number of cells
X1          = vec2mat(x(1:NN),N)';
k1          = 4e-4; %Abitrary parameter
k2          = 1e3; %Abitrary parameter
Di          = 6e3*pars(3); %Abitrary parameter
%%
%Spatial model
 zero = zeros(1,N);
 % 1. Fluxes in x-direction; zero fluxes near boundaries
 FI = -Di.*[zero;X1(2:N,:) - X1(1:N-1,:);zero]./dx;
 % Add flux gradient to rate of change
 dI = (FI(2:N+1,:) - FI(1:N,:))./dx;
 % 2. Fluxes in y-direction; zero fluxes near boundaries
 FI = -Di.*[zero',X1(:,2:N) - X1(:,1:N-1),zero']./dx;
 % Add flux gradient to rate of change
 dI  = - dI - (FI(:,2:N+1) - FI(:,1:N))./dx;
%%
dxX = k1.*matBac.*pars(4) - k2.*X1.^2 + dI;
dfx = dxX(:); %Merge equations
end
