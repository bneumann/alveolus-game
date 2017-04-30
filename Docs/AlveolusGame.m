%[n,nb,nummac,out] = AlveolusGame(pars)
%pars: [ 1.number of bacteria (1),
%        2.number of macrophages (3),
%        3.chemokine diffusion constant (6e3),
%        4.antigens per bacteria (1),
%        5.Movement in sessile phase (0.1),
%        6.Movement in flowing phase (3),
%        7.Radial flow (4.2e-2),
%        8.Bacteria dobling time (200),
%        9.Probability interchanged (0.999),
%       10.Bacteria saturation number (10),
%       11.Macrophage movement (2),
%       12.Phagocytosys rate (0.048),
%       13.Maximum bacteria per macrophage (50)
%       14.Distance to sense the metabolic gradient (30)
%       15.Sensitivity to feel the cytokine gradient (1e-6)]
%n: minutes at the end of the simulation
%nb: number of bacteria at the end of the simulation
%nummac: number of macrophages at the end of the simulation
%
%Multilevel model of the bacterial (Streptococcus pneumoniae) infection of
%an alveolus. It is simulated the stablishment of the infection. Bacteria
%interact with the alveolar macrophages and lung epithelial cells.
%
%Epithelial cells have intracellular signaling structure for inflammation
%through intracellular signaling pathway using ordinary differential equations
%(eqs).
%
%Bacteria and macrophages interact in the surfice of epithelial cells,
%modelled as an agent basd model (bacsMulti).
%
%bacsMulti:
%Mathematical simulation of bacterial groth and movement in a single
%alveolus together with macrophages movement and bacteria clearance.
%
%-Bacteria growth with a saturable process.
%-Bacteria jump stochasticaly between two liquid phases in alveolus: flowing
%phasde and stationary phase.
%-Coughing can be simulated, inactivated by default.
%-Bacteria move in a random walk way, faster in the flowing phase.
%-Macrophages move in a randomn walk way following the chemokine gradient
%produced by epithelial cells.
%-Macrophages eat bacteria. First attach and then there is a dynamics of
%phagogytosis. Macrophages die if the number of attached bacteria is very
%high.
%-There is a defined flow velocity of the mobile liquid phase.
%-Bacteria leave the alveolus when reach the border.
%
%Version 1.2 gaming (16.02.2017) by Guido Santos
%
function [n,nb,nummac,out] = AlveolusGame(pars)
global contador matBac
close all %Close previous figures
clc %Clear the screen
tic %Start counting time
if (exist('pars','var') == 0 || numel(pars) == 0)
    pars = ones(17,1); %Define the input if it doesn't exist
end
%Parameters to define
N                      = 11; %Numer of epithelial cells per row, [[PMID: 25360787]]
cel                    = 30; %Microm of epithelial cell (30 microM [[PMID: 25360787 & PMID: 7611375]])
incre                  = 5; %Interval of time between synchronization of models in minutes
nb                     = round(1*pars(1)); %Initial number of bacteria
nummac                 = round(1*pars(2)); %Number of macrophages, approximation from Bionumbers
FT                     = 24; %Final time for the model in hours
foco                   = 4; %Dispersion of initial bacteria in the alveolus
movie                  = 1; %If 1 show results and save movie
pantalla               = 0; %If 1 show the results in command windows
step1                  = 0.1*pars(5); %Movement rate in the sesile phase, assumed
step2                  = 3*pars(6); %Movemenet rate in the mobile phase, assumed
step3                  = 4.2e-2*pars(7); %min-1. Radial flow (4.2e-2) Fitted to alveolar clearance in figura 3B [[PMID: 17290033]] calculated in Excel parameters calculation.xlsx (sheet: alveoli flow)
%%
%Parameters defined
sensi                  = incre/60; %Interval of time between synchronization of models in hours
NN                     = N*N; %Number of cell in alveolus
tiempo                 = 0:sensi:FT; %Time for integration of the model
solver                 = @ode45; %Integration method
matriz                 = zeros(1,NN); %Initial conditions for the ODE model
dimens                 = (cel*N)/2; %Dimension of the alveoli in micrometers from Bionumbers (11 from 121 cells)
n                      = 0; %Initial number of iterations in minutes
out                    = 0; %Initial number of bacteria spreaded
pos                    = random('norm',0,dimens/foco,nb,2); %Normal distribution of initial bacteria
mac                    = dimens*(rand(nummac,2)*2 - 1); %Random localization of macrophages
neat                   = zeros(1,nummac); %Intrinsic macrophage time for bacteria phagocytosis
eaten                  = zeros(1,nummac); %Register of eated bacteria
c                      = 0; %Counting the iterations
nstop                  = 0; %Counting stoping time between the models
tstep                  = 0; %Counting the time steps between models
CHE                    = zeros(N,N,1); %Chemokine
final                  = 0;
step                   = ones(nb,1)*step1; %Movement rate in the sesile phase (step1)
%if movie == 1
%    writerObj1           = VideoWriter('Alveolus.avi'); %Create a movie
%    writerObj1.FrameRate = 3; %Define the frame rate
%    open(writerObj1); %Open the movie
%else
%    writerObj1 = []; %Create a movie
%end
%%
writerObj1 = [];
while (tstep < tiempo(end) && final == 0)
    %Run the models in steps
    tinit                                                                                            = nstop/60; %Initial time for the time step between models
    contador                                                                                         = 0; %Counting the integration steps
    [pos,out,n,nb,nummac,mac,neat,eaten,nstop,matBac,writerObj1,final,indMac,step,step1,step2,step3] = bacsMulti(pos,out,n,nb,nummac,mac,neat,eaten,nstop,CHE,writerObj1,N,cel,movie,final,pars,step,step1,step2,step3,pantalla); %Run the bacteria model
    nstop                                                                                            = nstop + incre; %Count the time step between models
    tstep                                                                                            = nstop/60; %In hours
    if tstep > tiempo(end)
        tstep = tiempo(end); %Cut the time to the maximum
    end
   tspan  = tinit:sensi:tstep; %Integration interval
   [t,y]  = feval(solver,@eqs,tspan,matriz,[],N,cel,pars); %Run the epithelial cell model
%   [t,y]  = ode45(@eqs, tspan, matriz,[],N,cel,pars);
   matriz = y(end,:); %Define the initial condition as the last condition of the previous run
   csvwrite(sprintf('matriz_%d.csv', n), vec2mat(matriz,11));
   csvwrite(sprintf('bac_%d.csv', n), matBac);
   c0     = 0; %Count the time steps
   %%
   %Join the integration steps
   for time = t'
       c          = c + 1;
       c0         = c0 + 1;
       CHE(:,:,c) = vec2mat(y(c0,1:NN),N);
   end
%   csvwrite(sprintf('che_%d.csv', n), CHE);
    %%
end
%Create the movie
if movie == 1
    close(writerObj1);
end
%%
end
%Epithelial cell model
function dfx = eqs(~,x, ~,N,cel,pars)
global contador matBac
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
%Bacteria model
function [pos,out,n,nb,nummac,mac,neat,eaten,nstop,matBac,writerObj1,final,indMac,step,step1,step2,step3] = bacsMulti(pos,out,n,nb,nummac,mac,neat,eaten,nstop,CHE,writerObj1,Nc,cel,movie,final,pars,step,step1,step2,step3,pantalla)
Itime = clock; %Initial time
%Biological parameters for alveolus and inputs
if exist('Nc','var') == 0
    Nc = 11; %Number of cells per row in alveolus
end
if exist('cel','var') == 0
    cel = 30; %Width of epitheliaol cell in microm
end
dimens = (cel*Nc)/2; %Dimension of the alveoli in micrometers from Bionumbers (11 from 121 cells)
if exist('nb','var') == 0
    nb = 1; %Initial number of bacteria
end
if exist('n','var') == 0
    n = 0; %Iterations
end
if exist('out','var') == 0
    out = 0; %initial number of bacteria spread
end
if exist('pos','var') == 0
    pos = random('norm',0,dimens/4,nb,2); %Normal distribution of initial bacteria
end
if exist('nummac','var') == 0
    nummac = 3; %Number of macrophages, approximation from Bionumbers
end
if exist('mac','var') == 0
    mac = dimens*(rand(nummac,2)*2 - 1); %Random localization of macrophages
end
if exist('neat','var') == 0
    neat = zeros(1,nummac); %Intrinsic macrophage time for bacteria phagocytosis
end
if exist('eaten','var') == 0
    eaten = zeros(1,nummac); %Register of eated bacteria
end
if exist('nstop','var') == 0
    nstop = 600; %Register of eated bacteria
end
if exist('CHE','var') == 0
    CHE = zeros(Nc,Nc,1); %Empty landscape of epithelial cells
end
if exist('writerObj1','var') == 0
    writerObj1 = VideoWriter('Alveolus.avi'); %Create a movie
    open(writerObj1); %Open a movie
end
limits             = [-dimens,dimens]; %Dimension of alveolus in nm (30 microM/cell [[PMID: 25360787 & PMID: 7611375]])
pos(pos > dimens)  = dimens; %No infection out of the alveolus
pos(pos < -dimens) = -dimens; %No infection out of the alveolus
front              = linspace(limits(1),limits(2),Nc + 1); %Define the frontiers of the epithelial cells
%%
%Biological parameters for bacteria
growth             = round(200*pars(8)); %min-1. 199.2 min doubling time (3.32 hours from [[pag 38 book: ISBN 978-1-908230-17-1]])Iterations per division (5e2)
probS              = 0.999*pars(9); %Probability of not being interchanged between phases (0.999) A lower value stablish the infection with low probability
cough              = 0; %If 1 activates coughing
probC              = 0.995; %Probability of not leave the alveolus from the mobile phase
maxbac             = 1e3; %Maximum number of bacteria in the alveolus simulated, not relevant
rad                = 1.5; %Region for determine the saturation of bacteria (1.5) Assuming two hexagonal layers
Mbac               = 10*pars(10); %Maximum number of bacteria inside the region to saturate the growth (10) Assuming two hexagonal layers
matBac             = zeros(Nc,Nc); %Define the bacteria matrix
%%
%Biological parameters for macrophages
macsiz = 10; %Macrophage size in micras (21 microm of diameter, from Wikipedia)
stepm  = 2*pars(11); %Movement of macrophages (2 micrm/min de [[PMID: 26202827]])
clebac = 0.048*pars(12); %Phagocytosys rate (min-1, calculated in Excel parameters calculation.xlsx (sheet: Phagocytosys rate) [[PMC266186]])
maxeat = 50*pars(13); %Maximum number of bacteria attached [[buscar]] %%% More than 70% of the macrophage perimeter covered of bacteria inside one minute
sensit = 1e-6*pars(15); %Sensitivity to feel the cytokine gradient [[buscar]].
indMac = ones(nummac,2); %Initial value for indexes of macrophages
%%
%Display parameters
fi      = 101; %Index for the figure
pintar  = 1; %If 1 displays the figure
iteplot = 10; %Number of iteration per plot
%%
% %Loop of simulation
if (nb < 1 || nb > maxbac || nummac < 1)
    final = 1; %Terminate the loop
end
if n == 0
   %Display the first condition
    if mod(n,iteplot) == 0
        if movie == 1
            if ishandle(fi*10) == 1
                close(fi*10) %Close previous figure
            end
            figure(fi*10) %Create figure
            set(fi*10,'Visible','off'); %Prevent display
            CHEt = CHE(:,:,end)'; %Transpose matrix
            imagesc(CHEt,[0 5e-4]) %Plot epithelial cell layer
            set(gca,'position',[0 0 1 1],'units','normalized') %Remove margins
            axis off %Clear axis
%            print(fi*10,'epithelial.png','-dpng') %Save image
%            img = imread('epithelial.png'); %Read the state as picture
            if pintar == 1
                figure(fi)
            else
                ff  = figure('visible','off');
            end
%            imagesc(limits,limits,img); %Plot the state of epithelial cells in the background
            hold on %Overlap graph
%            scatter(pos(:,1),pos(:,2),12,'MarkerFaceColor','r','MarkerEdgeColor','r',...
%                'MarkerFaceAlpha',.1,'MarkerEdgeAlpha',0) %Plot the bacteria with transparency
%            scatter(pos(:,1),pos(:,2),12,'MarkerFaceColor','r','MarkerEdgeColor','r') %Plot the bacteria with transparency
            plot(pos(:,1),pos(:,2),'.r','MarkerSize',20) %Plot the bacteria
            plot(mac(:,1),mac(:,2),'.c','MarkerSize',50) %Plot the macrophages
            hold off %Stop overlaping
            for m = 1:nummac
                text(mac(m,1),mac(m,2),num2str(eaten(m)),'HorizontalAlignment','center','FontSize',10); %Display the number of attached bacteria
            end
            xlim(limits) %Define the x axis limits
            ylim(limits) %Define the y axis limits
            title(strcat('Number of bacteria=',num2str(nb),'|Spreaded bacteria=',num2str(out),'|Hours=',num2str(floor(n/60)),'/24h')) %Number of bacteria in the title
            xlabel('Dimension (microm)') %x axis label
            ylabel('Dimension (microm)') %y axis label
%            if pintar == 1
%                frame1 = getframe(figure(fi)); %Save the figure as a frame
%            else
%                frame1 = getframe(ff); %Save the figure as a frame
%            end
%            writeVideo(writerObj1,frame1); %Write the frame in the movie
        end
        Ctime = clock; %Current time
        if round(Itime(5)) < 10
            minutosI = strcat('0',num2str(round(Itime(5)))); %Add a 0 if minutes are less than 10
        else
            minutosI = num2str(round(Itime(5))); %Save the time for minutes higher tha 10
        end
        if round(Ctime(5)) < 10
            minutosC = strcat('0',num2str(round(Ctime(5)))); %Add a 0 if minutes are less than 10
        else
            minutosC = num2str(round(Ctime(5))); %Save the time for minutes higher tha 10
        end
        horaI = strcat(num2str(Itime(4)),':',minutosI); %Save the time as a string
        horaC = strcat(num2str(Ctime(4)),':',minutosC); %Save the time as a string
        if pantalla == 1
            fprintf('Iteration: %g minutes (%g hours | %g days)\nAlveolar bacteria: %g\nSpreaded bacteria: %g\nMacrophages: %g\nInit time: %s\nCurrent time: %s\n',n,floor(n/60),floor(n/(60*24)),nb,out,nummac,horaI,horaC) %Display the results in the screen
            fprintf('\n') %Jump one line
        end
    end
    %%
end
while (nb > 0 && nb < maxbac && n < nstop && nummac > 0)
    %Display the current situation
    if mod(n,iteplot) == 0
        if movie == 1
            if ishandle(fi*10) == 1
                close(fi*10) %Close previous figure
            end
            figure(fi*10) %Create figure
            set(fi*10,'Visible','off'); %Prevent display
            CHEt = CHE(:,:,end)'; %Transpose matrix
            imagesc(CHEt,[0 5e-4]) %Plot epithelial cell layer
            set(gca,'position',[0 0 1 1],'units','normalized') %Remove margins
            axis off %Clear axis
%            print(fi*10,'epithelial.png','-dpng') %Save image
%            img = imread('epithelial.png'); %Read the state as picture
            if pintar == 1
                figure(fi)
            else
                ff  = figure('visible','off');
            end
%            imagesc(limits,limits,img); %Plot the state of epithelial cells in the background
            hold on %Overlap graph
%            scatter(pos(:,1),pos(:,2),12,'MarkerFaceColor','r','MarkerEdgeColor','r',...
%                'MarkerFaceAlpha',.1,'MarkerEdgeAlpha',0) %Plot the bacteria with transparency
%            scatter(pos(:,1),pos(:,2),12,'MarkerFaceColor','r','MarkerEdgeColor','r') %Plot the bacteria with transparency
            plot(mac(:,1),mac(:,2),'.c','MarkerSize',50) %Plot the macrophages
            hold off %Stop overlaping
            for m = 1:nummac
                text(mac(m,1),mac(m,2),num2str(eaten(m)),'HorizontalAlignment','center','FontSize',5); %Display the number of attached bacteria
            end
            xlim(limits) %Define the x axis limits
            ylim(limits) %Define the y axis limits
            title(strcat('Number of bacteria=',num2str(nb),'|Spreaded bacteria=',num2str(out),'|Hours=',num2str(floor(n/60)),'/24h')) %Number of bacteria in the title
            xlabel('Dimension (microm)') %x axis label
            ylabel('Dimension (microm)') %y axis label
%            if pintar == 1
%                frame1 = getframe(figure(fi)); %Save the figure as a frame
%            else
%                frame1 = getframe(ff); %Save the figure as a frame
%            end
%            writeVideo(writerObj1,frame1); %Write the frame in the movie
        end
        Ctime = clock; %Current time
        if round(Itime(5)) < 10
            minutosI = strcat('0',num2str(round(Itime(5)))); %Add a 0 if minutes are less than 10
        else
            minutosI = num2str(round(Itime(5))); %Save the time for minutes higher tha 10
        end
        if round(Ctime(5)) < 10
            minutosC = strcat('0',num2str(round(Ctime(5)))); %Add a 0 if minutes are less than 10
        else
            minutosC = num2str(round(Ctime(5))); %Save the time for minutes higher tha 10
        end
        horaI = strcat(num2str(Itime(4)),':',minutosI); %Save the time as a string
        horaC = strcat(num2str(Ctime(4)),':',minutosC); %Save the time as a string
        if pantalla == 1
            fprintf('Iteration: %g minutes (%g hours | %g days)\nAlveolar bacteria: %g\nSpreaded bacteria: %g\nMacrophages: %g\nInit time: %s\nCurrent time: %s\n',n,floor(n/60),floor(n/(60*24)),nb,out,nummac,horaI,horaC) %Display the results in the screen
            fprintf('\n') %Jump one line
        end
    end
    %%
    %Iterations
    n   = n + 1; %Count the iterations (minutes)
    nbP = nb; %Save the previous iteration
    %%
    %Bacterial saturable growth
%    sqdi             = squareform(pdist(pos)); %Calculate the distances
%    sqce             = sqdi*0; %Temporal variable
%    sqce(sqdi < rad) = 1; %Find short distances
%    cerca            = sum(sqce,2); %Count the close bacteria
%    crecen           = (cerca < Mbac); %Define the growing bacteria below the threshold
%    if isempty(crecen)
        crecen = nb; %If it's empty give one value
%    end
    if mod(n,growth) == 0
        pos(nb + 1:(nb + sum(crecen)),:)  = pos(crecen,:); %Bacteria divide
        step(nb + 1:(nb + sum(crecen)),1) = step(crecen,1); %Doubling the step vector
        nb                                = nb + sum(crecen); %Doubling the number of bacteria
    end
    %%
    %Interchange between phases
    step((rand(length(step),1) > probS & step == step1),1) = step2; %Jump to the mobile phase
    step((rand(length(step),1) > probS & step == step2),1) = step1; %Settle into the sessile phase
    %%
    %Simulating coughing
    if cough == 1
        fuera        = (rand(length(step),1) > probC & step == step2); %Count the bacteria being coughed
        pos(fuera,:) = []; %Remove bacteria
        step(fuera)  = []; %Remove stepsrand
        nb           = nb - sum(fuera); %Substract bacteria from the count
    end
    %%
    %Random movement of bacteria
    angl = rand(nb,1)*2*pi; %Random direction
    pos  = pos + [step.*cos(angl),step.*sin(angl)]; %Step into the direction defined
    %%
    %Movement of macrophages
    dist = zeros(length(pos(:,1)),nummac); %Define the distance matrix between bacteria and macrophages
    for m = 1:nummac
       dist(:,m) = sqrt((mac(m,1) - pos(:,1)).^2 + (mac(m,2) - pos(:,2)).^2); %Measure distance to macrophages
    end
    cCHE = CHE(:,:,end); %Current concentration of CHE
    angl = zeros(nummac,1); %Define the direction vector
    for m = 1:nummac
        angrec = [rand*2*pi;0;pi;pi/2;(3*pi)/2]; %Define the Manhattan angles
        fil    = find(abs(diff(front <= mac(m,1)))); %Row of macrophage in epithelial cells layer
        col    = find(abs(diff(front <= mac(m,2)))); %Column of macrophage in epithelial cells layer
        if isempty(fil)
            fil = Nc; %Prevent being out of the layer
        end
        if isempty(col)
            col = Nc; %Prevent being out of the layer
        end
        indmac            = [fil,col]; %Find the position of macrophages in epithelial layer
        vecin             = [indmac(1),indmac(2); %Look for the current cell
                             indmac(1) + 1,indmac(2); %Look for the rigth neighbour
                             indmac(1) - 1,indmac(2); %Look for the left neighbour
                             indmac(1),indmac(2) + 1; %Look for the down neighbour
                             indmac(1),indmac(2) - 1]; %Look for the up neighbour
        vecin(vecin < 1)  = 1; %Prevent the borders
        vecin(vecin > Nc) = Nc; %Prevent the borders
        vecin             = [cCHE(vecin(1,1),vecin(1,2));cCHE(vecin(2,1),vecin(2,2));cCHE(vecin(3,1),vecin(3,2));cCHE(vecin(4,1),vecin(4,2));cCHE(vecin(5,1),vecin(5,2))]; %Take the cytokine from the neighbours
        vecin             = round((vecin/sensit))*sensit; %Define the sensitivity to feel the gradient
        mover             = find(max(vecin) == vecin,1); %Find the maximum concentration
        angl(m,1)         = angrec(mover); %Define the directional movement
        if (min(dist(:,m)) < cel*pars(14) && numel(pos) > 0)
            [apol,~]  = cart2pol(pos(min(dist(:,m)) == dist(:,m),1) - mac(m,1),pos(min(dist(:,m)) == dist(:,m),2) - mac(m,2)); %Define the angle as the closest bacteria is is closer than one epithelial cell distance
            angl(m,1) = apol; %Save the angle
        end
    end
    mac                 = mac + [stepm.*cos(angl),stepm.*sin(angl)]; %Step into the direction defined
    mac(mac > dimens)   = dimens; %Define limits for the movement
    mac(mac < - dimens) = - dimens; %Define limits for the movement
    %%
    %Macrophages eat bacteria
    eaten = eaten + sum(dist < macsiz,1); %Count the attached bacteria per macrophages
    if sum(eaten > maxeat) ~= 0
        nummac = nummac - sum(eaten > maxeat); %Substract death macrophages
    end
    mac(eaten > maxeat,:)           = []; %Remove macrophages
    neat(eaten > maxeat)            = []; %Remove intrinsic time
    dist(:,eaten > maxeat)          = []; %Remove distance to bacteria
    eaten(eaten > maxeat)           = []; %Remove eated bacteria
    neat(sum(dist < macsiz,1) ~= 0) = 0; %Fix to 0 when macrophage eats bacteria
    eaten                           = eaten.*exp(-clebac.*neat); %Bacteria are phagocyted
    eaten                           = round(eaten); %Round number of bacteria attached
    pos(min(dist,[],2) < macsiz,:)  = []; %Remove bacteria attached to macrophages
    step(min(dist,[],2) < macsiz)   = []; %Remove steps
    if nummac > 0
        nb = nb - sum(min(dist,[],2) < macsiz); %Substract bacteria from the count
    end
    neat                            = neat + 1; %Count the intrinsic time
    %%
    %Movement through the mobile phase
    pos(step > step1,:) = pos(step > step1,:) + step3*pos(step > step1,:); %Radial flow
    %%
    %Bacteria leave the alveolus
    fuera        = sum(abs(pos) > dimens,2) > 0; %Count the bacteria out of the alveolus
    out          = out + sum(fuera); %Count the spreaded bacteria
    pos(fuera,:) = []; %Remove bacteria
    step(fuera)  = []; %Remove steps
    nb           = nb - sum(fuera); %Substract bacteria from the count
    %%
    mac
    %Count the bacteria per epithelial cell
    matBac = zeros(Nc,Nc); %Define the bacteria matrix
    for f = 1:Nc
        for c = 1:Nc
            if (f == 1 && c == 1)
                matBac(f,c) = sum(pos(step == step1,1) >= front(c) & pos(step == step1,1) <= front(c + 1) & pos(step == step1,2) >= front(f) & pos(step == step1,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
            elseif f == 1
                matBac(f,c) = sum(pos(step == step1,1) > front(c) & pos(step == step1,1) <= front(c + 1) & pos(step == step1,2) >= front(f) & pos(step == step1,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
            elseif c == 1
                matBac(f,c) = sum(pos(step == step1,1) >= front(c) & pos(step == step1,1) <= front(c + 1) & pos(step == step1,2) > front(f) & pos(step == step1,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
            else
                matBac(f,c) = sum(pos(step == step1,1) > front(c) & pos(step == step1,1) <= front(c + 1) & pos(step == step1,2) > front(f) & pos(step == step1,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
            end
        end
    end
    %%
    %Obtain the position of macrophages
    indMac = zeros(nummac,2); %Define the position indexes
    for ma = 1:nummac
        matMac = zeros(Nc,Nc); %Define the macrophage matrix
        for f = 1:Nc
            for c = 1:Nc
                if (f == 1 && c == 1)
                    matMac(f,c) = (mac(ma,1) >= front(c) & mac(ma,1) <= front(c + 1) & mac(ma,2) >= front(f) & mac(ma,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
                elseif f == 1
                    matMac(f,c) = (mac(ma,1) > front(c) & mac(ma,1) <= front(c + 1) & mac(ma,2) >= front(f) & mac(ma,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
                elseif c == 1
                    matMac(f,c) = (mac(ma,1) >= front(c) & mac(ma,1) <= front(c + 1) & mac(ma,2) > front(f) & mac(ma,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
                else
                    matMac(f,c) = (mac(ma,1) > front(c) & mac(ma,1) <= front(c + 1) & mac(ma,2) > front(f) & mac(ma,2) <= front(f + 1)); %Count the attached bacteria per epithelial cell
                end
            end
        end
    [I,J]        = ind2sub([Nc,Nc],find(matMac == 1));
    indMac(ma,:) = [I,J];
    end
end
end
%%
